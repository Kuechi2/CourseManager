using CourseManager.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CourseManager.Data // Prüfe, ob dein Namespace so heißt!
{
    public static class DbInitializer
    {
        public static async Task SeedData(IDbContextFactory<AppDataContext> factory)
        {
            // WICHTIG: Da wir hier im Seeding sind, haben wir oft noch keinen eingeloggten User.
            // Falls dein Context im OnModelCreating filtert, könnte er hier Dinge "verstecken".
            using var context = factory.CreateDbContext();
            await context.Database.MigrateAsync();   // ← war: EnsureCreatedAsync()

            // Wenn schon Schulen da sind, gehen wir davon aus, dass bereits geseedet wurde
            if (await context.Schools.AnyAsync()) return;

            try
            {
                // 1. Eine Beispiel-Schule erstellen
                var school = new School
                {
                    Id = Guid.NewGuid(),
                    Name = "Coders Academy",
                    City = "Königswinter",      // <--- Das hat gefehlt!
                    Address = "Musterstraße 1", // <--- Sicherstellen, dass das auch da ist
                    Email = "info@schule.de",   // <--- Und das
                    AccessCode = "START2024"    // <--- Und das
                };
                context.Schools.Add(school);

                // 2. Lehrer erstellen
                var prof = new Teacher
                {
                    Id = Guid.NewGuid(),
                    UserName = "lord.k@schule.de",
                    Email = "lord.k@schule.de",
                    NormalizedUserName = "LORD.K@SCHULE.DE",
                    NormalizedEmail = "LORD.K@SCHULE.DE",
                    FirstName = "Lord",
                    LastName = "K",
                    ShortName = "KÜC",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                var hasher = new PasswordHasher<Teacher>();
                prof.PasswordHash = hasher.HashPassword(prof, "Start123!");
                context.Users.Add(prof);
                 prof.ActiveSchoolId = school.Id; 

                // 4. Kurs anlegen (Muss die SchoolId bekommen!)
                var course = new Course
                {
                    Id = Guid.NewGuid(),
                    Title = "Informatik 101",
                    TeacherId = prof.Id,
                    SchoolId = school.Id // Zwingend erforderlich für den Filter!
                };

                // 5. Layout anlegen
                var layout = new SeatLayout
                {
                    Id = Guid.NewGuid(),
                    Name = "Standard",
                    IsActive = true,
                    CourseId = course.Id,
                    // Falls SeatLayout auch ISchoolEntity implementiert:
                    //SchoolId = school.Id 
                };

                context.Courses.Add(course);
                context.SeatLayouts.Add(layout);

                Console.WriteLine(">>> SEEDING MIT SCHUL-STRUKTUR ERFOLGREICH <<<");
                // 6. Ein paar Schüler für die Schule anlegen
                var students = new List<Person>
                {
                    new Person { Id = Guid.NewGuid(), FirstName = "Max",
                        LastName = "Mustermann", Gender = Person.EGender.Divers,
                        BirthDate = DateTime.Now.AddYears(-16),
                        SchoolId = school.Id,
                        CreatedByTeacherId = prof.Id,
                        CreatedAt = DateTime.UtcNow },
                    new Person { Id = Guid.NewGuid(), FirstName = "Emma",
                        LastName = "Beispiel", Gender = Person.EGender.Mädchen,
                        BirthDate = DateTime.Now.AddYears(-15),
                        SchoolId = school.Id,
                        CreatedByTeacherId = prof.Id,
                        CreatedAt = DateTime.UtcNow },
                    new Person { Id = Guid.NewGuid(), FirstName = "Linus",
                        LastName = "Torvalds", Gender = Person.EGender.Junge,
                        BirthDate = DateTime.Now.AddYears(-17),
                        SchoolId = school.Id,
                        CreatedByTeacherId = prof.Id,
                        CreatedAt = DateTime.UtcNow }
                };
                context.Students.AddRange(students);

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> SEEDING FEHLER: {ex.Message}");
                throw;
            }
        }
    }
    }