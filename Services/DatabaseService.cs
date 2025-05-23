using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using WpfApp14.Models;

namespace WpfApp14.Services
{
    public static class DatabaseService
    {
        private static SqliteConnection? _connection;

        public static void Initialize(string dbPath)
        {
            if (_connection == null)
            {
                var connString = new SqliteConnectionStringBuilder
                {
                    DataSource = dbPath,
                    Mode = SqliteOpenMode.ReadWriteCreate
                }.ToString();
                _connection = new SqliteConnection(connString);
            }

            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            using var cmd = _connection.CreateCommand();

            // Create Users table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL,
                IQ INTEGER NOT NULL,
                CorrectAnswerPercentage REAL NOT NULL
            );";
            cmd.ExecuteNonQuery();

            // Create Quizzes table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Quizzes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();

            // Check if IsProgramQuiz column exists and add it if missing
            cmd.CommandText = "PRAGMA table_info(Quizzes);";
            using var reader = cmd.ExecuteReader();
            bool hasIsProgramQuiz = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "IsProgramQuiz")
                {
                    hasIsProgramQuiz = true;
                    break;
                }
            }
            reader.Close();

            if (!hasIsProgramQuiz)
            {
                cmd.CommandText = "ALTER TABLE Quizzes ADD COLUMN IsProgramQuiz INTEGER NOT NULL DEFAULT 0;";
                cmd.ExecuteNonQuery();
            }

            // Create Questions table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Questions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                QuizId INTEGER NOT NULL,
                Text TEXT NOT NULL,
                TimerSeconds INTEGER NOT NULL,
                FOREIGN KEY(QuizId) REFERENCES Quizzes(Id)
            );";
            cmd.ExecuteNonQuery();

            // Create Answers table
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Answers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                QuestionId INTEGER NOT NULL,
                Text TEXT NOT NULL,
                IsCorrect INTEGER NOT NULL,
                FOREIGN KEY(QuestionId) REFERENCES Questions(Id)
            );";
            cmd.ExecuteNonQuery();

            // Initialize program quizzes
            InitializeProgramQuizzes();
        }

        private static void InitializeProgramQuizzes()
        {
            try
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Quizzes WHERE IsProgramQuiz = 1;";
                var count = (long)cmd.ExecuteScalar()!;
                if (count > 0) return;

                var quizzes = new[]
                {
                    new { Title = "Физика", Questions = GetPhysicsQuestions() },
                    new { Title = "Математика", Questions = GetMathematicsQuestions() },
                    new { Title = "Химия", Questions = GetChemistryQuestions() },
                    new { Title = "Қазақ тілі", Questions = GetKazakhLanguageQuestions() },
                    new { Title = "C# және HTML", Questions = GetCSharpHTMLQuestions() }
                };

                foreach (var quiz in quizzes)
                {
                    cmd.CommandText = "INSERT INTO Quizzes (Title, IsProgramQuiz) VALUES ($t, 1);";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$t", quiz.Title);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "SELECT last_insert_rowid();";
                    var quizId = (long)cmd.ExecuteScalar()!;

                    foreach (var q in quiz.Questions)
                    {
                        InsertQuestion((int)quizId, q.Text, q.TimerSeconds, q.Answers);
                    }
                }
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException($"Failed to initialize program quizzes: {ex.Message}", ex);
            }
        }

        public static int InsertOrUpdateUser(User user)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;

            if (user.Id == 0) // New user
            {
                cmd.CommandText = @"INSERT INTO Users (Username, Password, IQ, CorrectAnswerPercentage)
                    VALUES ($username, $password, $iq, $percentage);";
                cmd.Parameters.AddWithValue("$username", user.Username);
                cmd.Parameters.AddWithValue("$password", user.Password ?? string.Empty);
                cmd.Parameters.AddWithValue("$iq", user.IQ);
                cmd.Parameters.AddWithValue("$percentage", user.CorrectAnswerPercentage);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT last_insert_rowid();";
                user.Id = (int)(long)cmd.ExecuteScalar()!;
            }
            else // Update existing user
            {
                cmd.CommandText = @"UPDATE Users
                    SET Username = $username, Password = $password, IQ = $iq, CorrectAnswerPercentage = $percentage
                    WHERE Id = $id;";
                cmd.Parameters.AddWithValue("$id", user.Id);
                cmd.Parameters.AddWithValue("$username", user.Username);
                cmd.Parameters.AddWithValue("$password", user.Password ?? string.Empty);
                cmd.Parameters.AddWithValue("$iq", user.IQ);
                cmd.Parameters.AddWithValue("$percentage", user.CorrectAnswerPercentage); // Fixed typo
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return user.Id;
        }

        public static User? GetUser(int userId)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, Password, IQ, CorrectAnswerPercentage FROM Users WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    IQ = reader.GetInt32(3),
                    CorrectAnswerPercentage = reader.GetFloat(4)
                };
            }
            return null;
        }

        public static int InsertQuiz(string title, bool isProgramQuiz = false)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = "INSERT INTO Quizzes (Title, IsProgramQuiz) VALUES ($t, $p);";
            cmd.Parameters.AddWithValue("$t", title);
            cmd.Parameters.AddWithValue("$p", isProgramQuiz ? 1 : 0);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            var quizId = (long)cmd.ExecuteScalar()!;
            tx.Commit();

            return (int)quizId;
        }

        public static void InsertQuestion(int quizId, string text, int timerSeconds, (string Text, bool IsCorrect)[] answers)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = @"INSERT INTO Questions (QuizId, Text, TimerSeconds)
                VALUES ($q, $txt, $tm);";
            cmd.Parameters.AddWithValue("$q", quizId);
            cmd.Parameters.AddWithValue("$txt", text);
            cmd.Parameters.AddWithValue("$tm", timerSeconds);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT last_insert_rowid();";
            var questionId = (long)cmd.ExecuteScalar()!;

            foreach (var (answerText, isCorrect) in answers)
            {
                cmd.CommandText = @"INSERT INTO Answers (QuestionId, Text, IsCorrect)
                    VALUES ($qid, $atext, $corr);";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("$qid", questionId);
                cmd.Parameters.AddWithValue("$atext", answerText);
                cmd.Parameters.AddWithValue("$corr", isCorrect ? 1 : 0);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

        public static List<QuizInfo> GetUserQuizzes()
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            var result = new List<QuizInfo>();
            using var cmd = _connection.CreateCommand();

            cmd.CommandText = @"SELECT Q.Id, Q.Title, COUNT(Qu.Id) AS QuestionCount
                FROM Quizzes Q
                LEFT JOIN Questions Qu ON Q.Id = Qu.QuizId
                WHERE COALESCE(Q.IsProgramQuiz, 0) = 0
                GROUP BY Q.Id, Q.Title
                ORDER BY Q.Id;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new QuizInfo
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    QuestionCount = reader.GetInt32(2)
                });
            }

            return result;
        }

        public static List<QuizInfo> GetProgramQuizzes()
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            var result = new List<QuizInfo>();
            using var cmd = _connection.CreateCommand();

            cmd.CommandText = @"SELECT Q.Id, Q.Title, COUNT(Qu.Id) AS QuestionCount
                FROM Quizzes Q
                LEFT JOIN Questions Qu ON Q.Id = Qu.QuizId
                WHERE Q.IsProgramQuiz = 1
                GROUP BY Q.Id, Q.Title
                ORDER BY Q.Id;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new QuizInfo
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    QuestionCount = reader.GetInt32(2)
                });
            }

            return result;
        }

        public static void DeleteQuiz(int quizId)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            using var checkCmd = _connection.CreateCommand();
            checkCmd.CommandText = "SELECT COALESCE(IsProgramQuiz, 0) FROM Quizzes WHERE Id = $q;";
            checkCmd.Parameters.AddWithValue("$q", quizId);
            var isProgramQuiz = (long)checkCmd.ExecuteScalar()! == 1;

            if (isProgramQuiz)
            {
                throw new InvalidOperationException("Program quizzes cannot be deleted.");
            }

            using var tx = _connection.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;

            cmd.CommandText = @"DELETE FROM Answers
                WHERE QuestionId IN (
                    SELECT Id FROM Questions WHERE QuizId = $q
                );";
            cmd.Parameters.AddWithValue("$q", quizId);
            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM Questions WHERE QuizId = $q;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM Quizzes WHERE Id = $q;";
            cmd.ExecuteNonQuery();

            tx.Commit();
        }

        public static List<QuestionDataModel> GetQuestionsForQuiz(int quizId)
        {
            if (_connection == null) throw new InvalidOperationException("DB not initialized");

            var result = new List<QuestionDataModel>();

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Questions WHERE QuizId = $q;";
            cmd.Parameters.AddWithValue("$q", quizId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var question = new QuestionDataModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    QuizId = quizId,
                    Text = reader.GetString(reader.GetOrdinal("Text")),
                    TimerSeconds = reader.GetInt32(reader.GetOrdinal("TimerSeconds")),
                    Answers = new List<AnswerDataModel>()
                };

                using var cmdAns = _connection.CreateCommand();
                cmdAns.CommandText = "SELECT * FROM Answers WHERE QuestionId = $qid;";
                cmdAns.Parameters.AddWithValue("$qid", question.Id);

                using var ansReader = cmdAns.ExecuteReader();
                while (ansReader.Read())
                {
                    question.Answers.Add(new AnswerDataModel
                    {
                        Id = ansReader.GetInt32(ansReader.GetOrdinal("Id")),
                        QuestionId = question.Id,
                        Text = ansReader.GetString(ansReader.GetOrdinal("Text")),
                        IsCorrect = ansReader.GetInt32(ansReader.GetOrdinal("IsCorrect")) != 0
                    });
                }

                result.Add(question);
            }

            return result;
        }

        // Quiz question data
        private static (string Text, int TimerSeconds, (string Text, bool IsCorrect)[] Answers)[] GetPhysicsQuestions()
        {
            return new[]
            {
                (Text: "Жылдамдықтың өлшем бірлігі қандай?", TimerSeconds: 30, Answers: new[]
                {
                    ("Ньютон", false), ("Ватт", false), ("м/с", true), ("Келвин", false)
                }),
                (Text: "Дене массасы 5 кг болса, оның салмағы неге тең? (g = 10 м/с²)", TimerSeconds: 30, Answers: new[]
                {
                    ("50 Н", true), ("5 Н", false), ("50 Н", false), ("500 Н", false)
                }),
                (Text: "Ток күшінің өлшем бірлігі:", TimerSeconds: 30, Answers: new[]
                {
                    ("Вольт", false), ("Ом", false), ("Ампер", true), ("Джоуль", false)
                }),
                (Text: "Қуаттың формуласы:", TimerSeconds: 30, Answers: new[]
                {
                    ("F = ma", false), ("I = U/R", false), ("P = A/t", true), ("V = s/t", false)
                }),
                (Text: "Энергияның өлшем бірлігі:", TimerSeconds: 30, Answers: new[]
                {
                    ("Ватт", false), ("Джоуль", true), ("Ньютон", false), ("Ом", false)
                }),
                (Text: "1 сағатта неше секунд бар?", TimerSeconds: 30, Answers: new[]
                {
                    ("3600", true), ("60", false), ("600", false), ("1000", false)
                }),
                (Text: "Температураның халықаралық өлшем бірлігі:", TimerSeconds: 30, Answers: new[]
                {
                    ("Цельсий", false), ("Фаренгейт", false), ("Кельвин", true), ("Градус", false)
                }),
                (Text: "Жұмыстың өлшем бірлігі:", TimerSeconds: 30, Answers: new[]
                {
                    ("Ватт", false), ("Ом", false), ("Джоуль", true), ("Ампер", false)
                }),
                (Text: "Күшті өлшейтін құрал:", TimerSeconds: 30, Answers: new[]
                {
                    ("Барометр", false), ("Динамометр", true), ("Термометр", false), ("Амперметр", false)
                }),
                (Text: "Ньютонның екінші заңы:", TimerSeconds: 30, Answers: new[]
                {
                    ("F = ma", true), ("F = mv", false), ("E = mc²", false), ("P = F/S", false)
                }),
                (Text: "1 Ньютон неге тең?", TimerSeconds: 30, Answers: new[]
                {
                    ("1 кг", false), ("1 кг·м/с²", true), ("1 Дж", false), ("1 Вт", false)
                }),
                (Text: "Жарық жылдамдығы қандай?", TimerSeconds: 30, Answers: new[]
                {
                    ("300000 км/с", true), ("150000 км/с", false), ("100000 км/с", false), ("250000 км/с", false)
                }),
                (Text: "Серпімділік күші ненің әсерінен пайда болады?", TimerSeconds: 30, Answers: new[]
                {
                    ("Ауырлық", false), ("Жылдамдық", false), ("Деформация", true), ("Массаның", false)
                }),
                (Text: "Архимед күші қайда бағытталады?", TimerSeconds: 30, Answers: new[]
                {
                    ("Жоғары", true), ("Төмен", false), ("Қозғалыс бағытына", false), ("Артқа", false)
                }),
                (Text: "Электр кернеуінің өлшем бірлігі:", TimerSeconds: 30, Answers: new[]
                {
                    ("Ом", false), ("Ампер", false), ("Вольт", true), ("Джоуль", false)
                })
            };
        }

        private static (string Text, int TimerSeconds, (string Text, bool IsCorrect)[] Answers)[] GetMathematicsQuestions()
        {
            return new[]
            {
                (Text: "√49 мәні:", TimerSeconds: 30, Answers: new[]
                {
                    ("6", false), ("8", false), ("7", true), ("9", false)
                }),
                (Text: "15 * 4 неге тең?", TimerSeconds: 30, Answers: new[]
                {
                    ("45", false), ("50", false), ("60", true), ("55", false)
                }),
                (Text: "(a + b)^2 формуласын табыңыз:", TimerSeconds: 30, Answers: new[]
                {
                    ("a^2 + b^2", false), ("a^2 + 2ab - b^2", false), ("a^2 + 2ab + b^2", true), ("a^2 - b^2", false)
                }),
                (Text: "100 санының 25%-ы қанша?", TimerSeconds: 30, Answers: new[]
                {
                    ("50", false), ("25", true), ("75", false), ("10", false)
                }),
                (Text: "π мәні жуықтап:", TimerSeconds: 30, Answers: new[]
                {
                    ("2.14", false), ("3.14", true), ("4.13", false), ("5.14", false)
                }),
                (Text: "Екі теріс санның көбейтіндісі:", TimerSeconds: 30, Answers: new[]
                {
                    ("Теріс", false), ("Нөл", false), ("Оң", true), ("Білмеймін", false)
                }),
                (Text: "12 мен 8-дің ЕҮОБ:", TimerSeconds: 30, Answers: new[]
                {
                    ("2", false), ("4", true), ("8", false), ("6", false)
                }),
                (Text: "2^3 мәні:", TimerSeconds: 30, Answers: new[]
                {
                    ("6", false), ("8", true), ("9", false), ("10", false)
                }),
                (Text: "Тік бұрыш неше градус?", TimerSeconds: 30, Answers: new[]
                {
                    ("45", false), ("90", true), ("180", false), ("360", false)
                }),
                (Text: "Трапецияда неше қабырға бар?", TimerSeconds: 30, Answers: new[]
                {
                    ("2", false), ("3", false), ("4", true), ("5", false)
                }),
                (Text: "Квадраттың барлық қабырғалары:", TimerSeconds: 30, Answers: new[]
                {
                    ("әртүрлі", false), ("тең", true), ("бірдей емес", false), ("ұзын", false)
                }),
                (Text: "Логарифмнің негізі 10 болса, бұл қандай логарифм?", TimerSeconds: 30, Answers: new[]
                {
                    ("Натурал", false), ("Ондық", true), ("Бүтін", false), ("Қарапайым", false)
                }),
                (Text: "Қисықтың астындағы аудан қалай аталады?", TimerSeconds: 30, Answers: new[]
                {
                    ("Интеграл", true), ("Туынды", false), ("Формула", false), ("Координата", false)
                }),
                (Text: "Туындының негізгі мағынасы:", TimerSeconds: 30, Answers: new[]
                {
                    ("Өсу", false), ("Кему", false), ("Жылдамдық", false), ("Жылдамдық өзгерісі", true)
                }),
                (Text: "Санды бөлшектеу дегеніміз не?", TimerSeconds: 30, Answers: new[]
                {
                    ("Қосу", false), ("Азайту", false), ("Бөлу", true), ("Қарапайым түрге келтіру", false)
                })
            };
        }

        private static (string Text, int TimerSeconds, (string Text, bool IsCorrect)[] Answers)[] GetChemistryQuestions()
        {
            return new[]
            {
                (Text: "Судың химиялық формуласы:", TimerSeconds: 30, Answers: new[]
                {
                    ("H2O", true), ("CO2", false), ("NaCl", false), ("O2", false)
                }),
                (Text: "Na элементінің аты:", TimerSeconds: 30, Answers: new[]
                {
                    ("Кальций", false), ("Натрий", true), ("Магний", false), ("Темір", false)
                }),
                (Text: "Периодтық жүйені кім жасады?", TimerSeconds: 30, Answers: new[]
                {
                    ("Ньютон", false), ("Менделеев", true), ("Эйнштейн", false), ("Фарадей", false)
                }),
                (Text: "Қышқылдарға тән дәм:", TimerSeconds: 30, Answers: new[]
                {
                    ("Тәтті", false), ("Ащы", false), ("Қышқыл", true), ("Тұзды", false)
                }),
                (Text: "HCl қандай қышқыл?", TimerSeconds: 30, Answers: new[]
                {
                    ("Сірке", false), ("Тұз қышқылы", true), ("Сірне", false), ("Азот", false)
                }),
                (Text: "H2 + O2 → H2O бұл қандай реакция?", TimerSeconds: 30, Answers: new[]
                {
                    ("Айырылу", false), ("Қосылу", true), ("Алмасу", false), ("Тотығу", false)
                }),
                (Text: "pH 7 ненені білдіреді?", TimerSeconds: 30, Answers: new[]
                {
                    ("Қышқылдық", false), ("Сілтілік", false), ("Бейтарап", true), ("Қатты", false)
                }),
                (Text: "H2SO4 қандай қышқыл?", TimerSeconds: 30, Answers: new[]
                {
                    ("Азот", false), ("Фосфор", false), ("Күкірт", true), ("Сірке", false)
                }),
                (Text: "Ag элементі:", TimerSeconds: 30, Answers: new[]
                {
                    ("Алтын", false), ("Күміс", true), ("Мыс", false), ("Қалайы", false)
                }),
                (Text: "NaCl не?", TimerSeconds: 30, Answers: new[]
                {
                    ("Қант", false), ("Тұз", true), ("Көмір", false), ("Су", false)
                }),
                (Text: "FeO темірдің қандай тотығу дәрежесі?", TimerSeconds: 30, Answers: new[]
                {
                    ("+1", false), ("+2", true), ("+3", false), ("+4", false)
                }),
                (Text: "Газ күйіндегі ең жеңіл элемент:", TimerSeconds: 30, Answers: new[]
                {
                    ("Оттек", false), ("Азот", false), ("Сутек", true), ("Көміртек", false)
                }),
                (Text: "Оксидтер не?", TimerSeconds: 30, Answers: new[]
                {
                    ("Қышқыл", false), ("Тұз", false), ("Су", false), ("Оттекпен қосылыстар", true)
                }),
                (Text: "Тотығу – бұл:", TimerSeconds: 30, Answers: new[]
                {
                    ("Электрон қосу", false), ("Электрон жоғалту", true), ("Су қосу", false), ("Тұз қосу", false)
                }),
                (Text: "CuSO4 – бұл:", TimerSeconds: 30, Answers: new[]
                {
                    ("Тұз", true), ("Қышқыл", false), ("Сілті", false), ("Оксид", false)
                })
            };
        }

        private static (string Text, int TimerSeconds, (string Text, bool IsCorrect)[] Answers)[] GetKazakhLanguageQuestions()
        {
            return new[]
            {
                (Text: "Сөйлем дегеніміз не?", TimerSeconds: 30, Answers: new[]
                {
                    ("Сөз тіркесі", false), ("Ойды білдіретін сөз не сөз тіркесі", true), ("Сөз", false), ("Жай сөйлем", false)
                }),
                (Text: "Зат есім қандай сұрақтарға жауап береді?", TimerSeconds: 30, Answers: new[]
                {
                    ("Не істеді?", false), ("Қандай?", false), ("Қайда?", false), ("Кім? Не?", true)
                }),
                (Text: "Етістік қандай сөз табы?", TimerSeconds: 30, Answers: new[]
                {
                    ("Сан", false), ("Зат", false), ("Қимылды білдіреді", true), ("Сын", false)
                }),
                (Text: "Сын есім нені білдіреді?", TimerSeconds: 30, Answers: new[]
                {
                    ("Қимыл", false), ("Зат", false), ("Сан", false), ("Сапа, түр-түсті", true)
                }),
                (Text: "Жіктік жалғау қай сөз табына жалғанады?", TimerSeconds: 30, Answers: new[]
                {
                    ("Зат есім", false), ("Сын есім", false), ("Етістік", true), ("Үстеу", false)
                }),
                (Text: "Көптік жалғауын табыңыз:", TimerSeconds: 30, Answers: new[]
                {
                    ("-лар", true), ("-дың", false), ("-мен", false), ("-ға", false)
                }),
                (Text: "«Ол мектепке барды» сөйлемінің тұрлаулы мүшелері:", TimerSeconds: 30, Answers: new[]
                {
                    ("Ол, мектепке", false), ("Ол, барды", true), ("Мектепке, барды", false), ("Барды", false)
                }),
                (Text: "«Көк» сөзі қандай сөз табы?", TimerSeconds: 30, Answers: new[]
                {
                    ("Сан есім", false), ("Зат есім", false), ("Сын есім", true), ("Есімдік", false)
                }),
                (Text: "«Жақсы» сөзінің антонимі:", TimerSeconds: 30, Answers: new[]
                {
                    ("Таза", false), ("Жаман", true), ("Қара", false), ("Ұзын", false)
                }),
                (Text: "Сөз таптарының саны:", TimerSeconds: 30, Answers: new[]
                {
                    ("7", false), ("8", false), ("9", true), ("10", false)
                }),
                (Text: "Септік жалғау нешеге бөлінеді?", TimerSeconds: 30, Answers: new[]
                {
                    ("3", false), ("5", false), ("6", false), ("7", true)
                }),
                (Text: "«Мен» сөзі қандай есімдік?", TimerSeconds: 30, Answers: new[]
                {
                    ("Жалпылау", false), ("Жіктеу", true), ("Сұрау", false), ("Сілтеу", false)
                }),
                (Text: "Сан есімге мысал:", TimerSeconds: 30, Answers: new[]
                {
                    ("Көк", false), ("Үш", true), ("Барды", false), ("Мен", false)
                }),
                (Text: "«Тез» сөзі қандай сөз табы?", TimerSeconds: 30, Answers: new[]
                {
                    ("Үстеу", true), ("Сын есім", false), ("Етістік", false), ("Зат есім", false)
                }),
                (Text: "Буынның неше түрі бар?", TimerSeconds: 30, Answers: new[]
                {
                    ("2", false), ("3", true), ("4", false), ("5", false)
                })
            };
        }

        private static (string Text, int TimerSeconds, (string Text, bool IsCorrect)[] Answers)[] GetCSharpHTMLQuestions()
        {
            return new[]
            {
                (Text: "C# қай платформаға арналған?", TimerSeconds: 30, Answers: new[]
                {
                    (".NET", true), ("JVM", false), ("Python", false), ("Linux", false)
                }),
                (Text: "Мәліметтер типі қандай?", TimerSeconds: 30, Answers: new[]
                {
                    ("string", true), ("number", false), ("char", false), ("text", false)
                }),
                (Text: "Цикл операторы:", TimerSeconds: 30, Answers: new[]
                {
                    ("if", false), ("for", true), ("print", false), ("switch", false)
                }),
                (Text: "Айнымалыны қалай жариялайды?", TimerSeconds: 30, Answers: new[]
                {
                    ("int a;", true), ("a int;", false), ("declare int a;", false), ("set a = int;", false)
                }),
                (Text: "Мәнді консольға шығару үшін:", TimerSeconds: 30, Answers: new[]
                {
                    ("echo", false), ("print", false), ("Console.WriteLine", true), ("write", false)
                }),
                (Text: "Массив құру жолы:", TimerSeconds: 30, Answers: new[]
                {
                    ("int[] arr;", true), ("list<int> arr;", false), ("array int arr;", false), ("set arr[]", false)
                }),
                (Text: "try...catch не үшін?", TimerSeconds: 30, Answers: new[]
                {
                    ("Шарт тексеру", false), ("Қате өңдеу", true), ("Цикл", false), ("Кіру", false)
                }),
                (Text: "Санды мәтінге түрлендіру:", TimerSeconds: 30, Answers: new[]
                {
                    ("Convert.ToString()", true), ("ToText()", false), ("Text()", false), ("as string", false)
                }),
                (Text: "class дегеніміз не?", TimerSeconds: 30, Answers: new[]
                {
                    ("Айнымалы", false), ("Мәлімет типі", false), ("Объект шаблоны", true), ("Цикл", false)
                }),
                (Text: "Метод дегеніміз:", TimerSeconds: 30, Answers: new[]
                {
                    ("Класс", false), ("Функция", true), ("Айнымалы", false), ("Қате", false)
                }),
                (Text: "Main() не үшін қажет?", TimerSeconds: 30, Answers: new[]
                {
                    ("Басты код", true), ("Қате өңдеу", false), ("Айнымалы", false), ("Цикл", false)
                }),
                (Text: "Жаңа объект жасау:", TimerSeconds: 30, Answers: new[]
                {
                    ("new Object()", true), ("Object.create()", false), ("create Object", false), ("new()", false)
                }),
                (Text: "== не үшін қолданылады?", TimerSeconds: 30, Answers: new[]
                {
                    ("Қосу", false), ("Салыстыру", true), ("Ажырату", false), ("Шығару", false)
                }),
                (Text: "&& операторы не істейді?", TimerSeconds: 30, Answers: new[]
                {
                    ("НЕМЕСЕ", false), ("ЖӘНЕ", true), ("ЕМЕС", false), ("БӨЛУ", false)
                }),
                (Text: "HTML толық атауы:", TimerSeconds: 30, Answers: new[]
                {
                    ("HyperText Markup Language", true), ("HighText Machine Language", false), ("HyperTool Multi Language", false), ("HyperText Markdown Language", false)
                })
            };
        }

        public class QuizInfo
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public int QuestionCount { get; set; }
        }

        public class QuestionDataModel
        {
            public int Id { get; set; }
            public int QuizId { get; set; }
            public string Text { get; set; } = string.Empty;
            public int TimerSeconds { get; set; }
            public List<AnswerDataModel> Answers { get; set; } = new();
        }

        public class AnswerDataModel
        {
            public int Id { get; set; }
            public int QuestionId { get; set; }
            public string Text { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
        }
    }
}   