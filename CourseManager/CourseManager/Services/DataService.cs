using Microsoft.EntityFrameworkCore;
namespace CourseManager.Data
{
    public class StudentService : IStudentService
    {
        private readonly IDbContextFactory<AppDataContext> _dbFactory;
        public event Action? OnChanged;
        private Guid CurrentTeacherId =>
            Guid.Parse(_httpContextAccessor.HttpContext?.User.
                FindFirst(System.Security.Claims.
                ClaimTypes.NameIdentifier)?.Value   //Zieht zu Beginn den User aus dem Kontext ODER
               ?? throw new UnauthorizedAccessException());     //Wirft ein Unauthorized
        private readonly IHttpContextAccessor _httpContextAccessor;
        public StudentService(IDbContextFactory<AppDataContext> dbFactory, IHttpContextAccessor httpContextAccessor)
        {
            _dbFactory = dbFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SaveStudentAsync(Person student)
        {
            Console.WriteLine($"[TRACE] SaveStudentAsync aufgerufen für: {student.FirstName} {student.LastName}");
            using var context = _dbFactory.CreateDbContext();
            var dbStudent = await context.Students.FirstOrDefaultAsync(s => s.Id == student.Id);
            if (dbStudent == null)
            {
                // --- LOGIK FÜR NEUANLAGE ---
                bool exists = await context.Students.AnyAsync(s =>
                    s.FirstName.ToLower() == student.FirstName.ToLower() &&
                    s.LastName.ToLower() == student.LastName.ToLower() &&
                    s.BirthDate.Date == student.BirthDate.Date);
                if (exists) throw new InvalidOperationException("Diese Person existiert bereits.");
                if (student.Id == Guid.Empty) student.Id = Guid.NewGuid();
                student.SchoolId = context.GetSchoolId();
                context.Students.Add(student);
            }
            else
            {
                // --- LOGIK FÜR UPDATE ---
                Console.WriteLine($"[TRACE] Aktualisiere Student: {student.FirstName} {student.LastName}");
                student.SchoolId = context.GetSchoolId();
                context.Entry(dbStudent).CurrentValues.SetValues(student);
            }
            await context.SaveChangesAsync();
            OnChanged?.Invoke();
        }
        public async Task<List<Person>> GetStudentsAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Students.OrderBy(s => s.LastName).ToListAsync();
        }
        /*
         * Courses Services
         */
        public async Task<List<Course>> GetCoursesAsync()
        {

            using var context = _dbFactory.CreateDbContext();
            return await context.Courses.Where(c => c.TeacherId == CurrentTeacherId).ToListAsync();
        }
        public async Task<List<Course>> GetCoursesWithParticipantsAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Courses
                .Where(c => c.TeacherId == CurrentTeacherId)
                .Include(c => c.Enrollments!)
                    .ThenInclude(e => e.Person)
                .Include(c => c.SeatLayouts!)
                    .ThenInclude(l => l.Seats!)
                        .ThenInclude(s => s.Participant) // Wer sitzt hier?
                            .ThenInclude(p => p!.Person) // Wie heißt die Person?
                .ToListAsync();
        }
        public async Task<Course?> GetCourseWithParticipantsAsync(Guid id)
        {
            using var context = _dbFactory.CreateDbContext();
            var course = await context.Courses
                .Include(c => c.Enrollments!)
                    .ThenInclude(e => e.Person)
                .Include(c => c.SeatLayouts!)
                    .ThenInclude(l => l.Seats!)
                        .ThenInclude(s => s.Participant)
                            .ThenInclude(p => p!.Person)
                .FirstOrDefaultAsync(c => c.Id == id);

            // DEBUG-LOGS HIER:
            if (course != null && course.SeatLayouts != null)
            {
                var layout = course.SeatLayouts.FirstOrDefault(l => l.IsActive) ?? course.SeatLayouts.FirstOrDefault();
                if (layout != null)
                {
                    Console.WriteLine($"[DEBUG] Prüfe Layout: {layout.Name}");
                    foreach (var s in layout.Seats!)
                    {
                        Console.WriteLine($"[DEBUG] Sitz {s.Id} an {s.PosX}/{s.PosY}: " +
                                          $"Participant-ID geladen: {s.CourseParticipantId}, " +
                                          $"Objekt da: {s.Participant != null}, " +
                                          $"Person da: {s.Participant?.Person != null}");
                    }
                }
            }

            return course;
        }
        public async Task SaveCourseAsync(Course course)
        {
            Console.WriteLine($"[TRACE] Start SaveCourseAsync für Kurs: {course.Title}");
            using var context = _dbFactory.CreateDbContext();

            var dbCourse = await context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.SeatLayouts)
                    .ThenInclude(l => l.Seats)
                .FirstOrDefaultAsync(c => c.Id == course.Id);

            if (dbCourse == null)
            {
                Console.WriteLine("[TRACE] Kurs neu -> Add");
                context.Courses.Add(course);
            }
            else
            {
                // Basis-Daten aktualisieren
                context.Entry(dbCourse).CurrentValues.SetValues(course);

                // --- ENROLLMENTS SYNC ---
                SyncEnrollments(context, dbCourse, course);

                // --- SEAT LAYOUTS SYNC ---
                SyncSeatLayouts(context, dbCourse, course);
            }

            try
            {
                Console.WriteLine("[TRACE] Rufe SaveChangesAsync auf...");
                var affectedRows = await context.SaveChangesAsync();
                Console.WriteLine($"[TRACE] Erfolg! Betroffene Zeilen: {affectedRows}");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("!!! CONCURRENCY FEHLER !!!");
                foreach (var entry in ex.Entries)
                {
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    var entityType = entry.Entity.GetType().Name;

                    if (databaseValues == null)
                    {
                        Console.WriteLine($"[ERROR] {entityType} existiert nicht in DB!");
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] {entityType} hat veralteten RowVersion!");
                    }
                }
                throw;
            }
        }
 
        // --- HILFSMETHODE: Seat Layouts synchronisieren ---
        private void SyncSeatLayouts(AppDataContext context, Course dbCourse, Course uiCourse)
        {
            // A. Layouts löschen, die im UI entfernt wurden
            foreach (var dbLayout in dbCourse.SeatLayouts.ToList())
            {
                if (!uiCourse.SeatLayouts.Any(l => l.Id == dbLayout.Id))
                {
                    Console.WriteLine($"[TRACE] Lösche Layout: {dbLayout.Name}");
                    context.SeatLayouts.Remove(dbLayout);
                }
            }

            // B. Layouts hinzufügen oder aktualisieren
            foreach (var uiLayout in uiCourse.SeatLayouts)
            {
                var dbLayout = dbCourse.SeatLayouts.FirstOrDefault(l => l.Id == uiLayout.Id);

                if (dbLayout == null)
                {
                    // --- NEUES LAYOUT ---
                    Console.WriteLine($"[TRACE] Erstelle neues Layout: {uiLayout.Name}");

                    uiLayout.CourseId = dbCourse.Id;
                    dbCourse.SeatLayouts.Add(uiLayout);

                    // ✅ WICHTIG: Layout + alle Seats als Added markieren
                    context.Entry(uiLayout).State = EntityState.Added;
                    foreach (var seat in uiLayout.Seats ?? new())
                    {
                        context.Entry(seat).State = EntityState.Added;
                    }
                }
                else
                {
                    // --- EXISTIERENDES LAYOUT UPDATEN ---
                    Console.WriteLine($"[TRACE] Update Layout: {uiLayout.Name}");
                    context.Entry(dbLayout).CurrentValues.SetValues(uiLayout);

                    // Seats synchronisieren
                    SyncSeats(context, dbLayout, uiLayout);
                }
            }
        }

        // --- HILFSMETHODE: Seats innerhalb eines Layouts synchronisieren ---
        private void SyncSeats(AppDataContext context, SeatLayout dbLayout, SeatLayout uiLayout)
        {
            // A. Seats löschen, die im UI entfernt wurden
            foreach (var dbSeat in dbLayout.Seats.ToList())
            {
                if (!uiLayout.Seats.Any(s => s.Id == dbSeat.Id))
                {
                    Console.WriteLine($"[TRACE] Lösche Seat: {dbSeat.Id}");
                    context.Seats.Remove(dbSeat);
                }
            }

            // B. Seats hinzufügen oder aktualisieren
            foreach (var uiSeat in uiLayout.Seats)
            {
                var dbSeat = dbLayout.Seats.FirstOrDefault(s => s.Id == uiSeat.Id);

                if (dbSeat == null)
                {
                    Console.WriteLine($"[TRACE] Neuer Seat in bestehendem Layout");
                    uiSeat.SeatLayoutId = dbLayout.Id;
                    dbLayout.Seats.Add(uiSeat);
                    context.Entry(uiSeat).State = EntityState.Added; // ✅ Explizit Added
                }
                else
                {
                    // ✅ WICHTIG: Participant-ID aktualisieren!
                    dbSeat.PosX = uiSeat.PosX;
                    dbSeat.PosY = uiSeat.PosY;
                    dbSeat.CourseParticipantId = uiSeat.CourseParticipantId; // Das war dein Kommentar im Frontend!
                }
            }
        }

        // --- ENROLLMENT SYNC (bereits vorhanden, aber kleiner Fix) ---
        private void SyncEnrollments(AppDataContext context, Course dbCourse, Course uiCourse)
        {
            Console.WriteLine("[TRACE] Synchronisiere Enrollments...");

            // A. Entferne gelöschte Enrollments
            var uiPersonIds = uiCourse.Enrollments?.Select(e => e.PersonId).ToHashSet() ?? new();
            var toRemove = dbCourse.Enrollments.Where(e => !uiPersonIds.Contains(e.PersonId)).ToList();

            foreach (var rem in toRemove)
            {
                Console.WriteLine($"[TRACE] Entferne Enrollment: {rem.PersonId}");
                context.Remove(rem);
            }

            // B. Füge neue Enrollments hinzu oder update
            foreach (var uiEnroll in uiCourse.Enrollments ?? new())
            {
                var dbEnroll = dbCourse.Enrollments.FirstOrDefault(e => e.PersonId == uiEnroll.PersonId);

                if (dbEnroll == null)
                {
                    Console.WriteLine($"[TRACE] Neues Enrollment: {uiEnroll.PersonId}");
                    uiEnroll.CourseId = dbCourse.Id;
                    uiEnroll.Person = null; // ✅ Wichtig: Verhindert Duplikat in Students-Tabelle
                    dbCourse.Enrollments.Add(uiEnroll);
                }
                else
                {
                    // Update nur relevante Felder (nicht die ID!)
                    dbEnroll.PosX = uiEnroll.PosX;
                    dbEnroll.PosY = uiEnroll.PosY;
                }
            }
        }
        private void SyncLayoutsAndSeats(AppDataContext context, Course dbCourse, Course uiCourse)
        {
            // A. Löschen: Layouts, die im UI nicht mehr da sind
            foreach (var dbLayout in dbCourse.SeatLayouts.ToList())
            {
                if (!uiCourse.SeatLayouts.Any(l => l.Id == dbLayout.Id))
                    context.SeatLayouts.Remove(dbLayout);
            }

            // B. Update / Hinzufügen
            foreach (var uiLayout in uiCourse.SeatLayouts)
            {
                var dbLayout = dbCourse.SeatLayouts.FirstOrDefault(l => l.Id == uiLayout.Id);
                if (dbLayout == null)
                {
                    dbCourse.SeatLayouts.Add(uiLayout); // EF erkennt neue IDs automatisch
                }
                else
                {
                    context.Entry(dbLayout).CurrentValues.SetValues(uiLayout);

                    // Sitze innerhalb des Layouts synchronisieren
                    // Löschen verwaister Sitze
                    foreach (var dbSeat in dbLayout.Seats.ToList())
                    {
                        if (!uiLayout.Seats.Any(s => s.Id == dbSeat.Id))
                            context.Seats.Remove(dbSeat);
                    }
                    // Update/Add Sitze
                    foreach (var uiSeat in uiLayout.Seats)
                    {
                        var dbSeat = dbLayout.Seats.FirstOrDefault(s => s.Id == uiSeat.Id);
                        if (dbSeat == null)
                            dbLayout.Seats.Add(uiSeat);
                        else
                            context.Entry(dbSeat).CurrentValues.SetValues(uiSeat);
                    }
                }
            }
        }
        private void SyncEnrollments(DbContext context, Course dbCourse, Course uiCourse)
        {
            // 1. Entferne Schüler, die im UI gelöscht wurden
            var toRemove = dbCourse.Enrollments
                .Where(dbE => !uiCourse.Enrollments.Any(uiE => uiE.PersonId == dbE.PersonId))
                .ToList();
            context.Set<CourseParticipant>().RemoveRange(toRemove);

            // 2. Füge neue Schüler hinzu
            foreach (var uiE in uiCourse.Enrollments)
            {
                if (!dbCourse.Enrollments.Any(dbE => dbE.PersonId == uiE.PersonId))
                {
                    // Sicherstellen, dass die Person dem Context bekannt ist, aber nicht neu erstellt wird
                    if (uiE.Person != null) context.Set<Person>().Attach(uiE.Person);
                    dbCourse.Enrollments.Add(uiE);
                }
            }
        }
        public async Task<bool> DeleteCourseAsync(Guid id)
        {
            using var context = _dbFactory.CreateDbContext();
            var c = await context.Courses.FindAsync(id);
            if (c == null) return false;

            context.Courses.Remove(c);
            await context.SaveChangesAsync();
            return true;
        }
        /*
         * Teacher Save
         */
        public async Task<List<TeacherDto>> GetTeachersAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            // ✅ SCHRITT 1: Entities OHNE Projection laden
            var rawTeachers = await context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .ToListAsync();

            Console.WriteLine("========== RAW ENTITIES FROM DB ==========");
            foreach (var t in rawTeachers)
            {
                Console.WriteLine($"Email: {t.Email}, IsApproved: {t.IsApproved}, Type: {t.IsApproved.GetType()}");
            }
            Console.WriteLine("==========================================");

            // ✅ SCHRITT 2: Jetzt projizieren
            var dtos = rawTeachers.Select(t => new TeacherDto
            {
                Id = t.Id,
                FirstName = t.FirstName,
                LastName = t.LastName,
                ShortName = t.ShortName,
                Email = t.Email,
                ActiveSchoolId = t.ActiveSchoolId,
                IsAdmin = t.IsAdmin,
                IsApproved = t.IsApproved,
                PointsBias = t.PointsBias
            }).ToList();

            Console.WriteLine("========== AFTER DTO PROJECTION ==========");
            foreach (var dto in dtos)
            {
                Console.WriteLine($"Email: {dto.Email}, IsApproved: {dto.IsApproved}, Type: {dto.IsApproved.GetType()}");
            }
            Console.WriteLine("==========================================");

            return dtos;
        }
        public async Task<List<TeacherDto>> GetTeachersAsync_Orginal()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Users
                .IgnoreQueryFilters() // WICHTIG: Alle Lehrer sehen, auch die von anderen Schulen!
        .Select(t => new TeacherDto
        {
            Id = t.Id,
            FirstName = t.FirstName,
            LastName = t.LastName,
            ShortName = t.ShortName,
            Email = t.Email,
            ActiveSchoolId = t.ActiveSchoolId,
            IsAdmin = t.IsAdmin,
            IsApproved = t.IsApproved,
            PointsBias = t.PointsBias,
            
        })
        .ToListAsync();
        }

        public async Task OLD_AddTeacher(TeacherDto dto)
        {
            using var context = _dbFactory.CreateDbContext();

            // Mapping: DTO -> Entity
            var teacher = new Teacher
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                ShortName = dto.ShortName == null ? "KEIN" : dto.ShortName,
                Email = dto.Email,
                UserName = dto.Email ?? dto.LastName // Identity braucht zwingend einen UserName!
            };

            context.Users.Add(teacher);
            await context.SaveChangesAsync();
            OnChanged?.Invoke();
        }
        public async Task AddTeacher(TeacherDto dto)
        {
            using var context = _dbFactory.CreateDbContext();

            // 1. Suche direkt mit der Guid (Kein .ToString()!)
            var existingTeacher = await context.Users
                .FirstOrDefaultAsync(t => t.Id == dto.Id || t.Email == dto.Email);

            if (existingTeacher != null)
            {
                // UPDATE
                existingTeacher.FirstName = dto.FirstName;
                existingTeacher.LastName = dto.LastName;
                existingTeacher.ShortName = dto.ShortName ?? "KEIN";
                existingTeacher.Email = dto.Email;
                existingTeacher.PointsBias = dto.PointsBias;
                existingTeacher.NormalizedEmail = dto.Email?.ToUpper();
                existingTeacher.UserName = dto.Email ?? dto.LastName;
                existingTeacher.IsAdmin = dto.IsAdmin;
                existingTeacher.IsApproved = dto.IsApproved;
                existingTeacher.NormalizedUserName = existingTeacher.UserName?.ToUpper();
                existingTeacher.ActiveSchoolId = dto.ActiveSchoolId;
                existingTeacher.SchoolId = dto.ActiveSchoolId; // WICHTIG: SchoolId immer mit updaten!
                context.Users.Update(existingTeacher);
            }
            else
            {
                // INSERT
                var newTeacher = new Teacher
                {
                    // Hier nutzen wir die Guid direkt
                    Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    ShortName = dto.ShortName ?? "KEIN",
                    Email = dto.Email,
                    NormalizedEmail = dto.Email?.ToUpper(),
                    UserName = dto.Email ?? dto.LastName,
                    IsAdmin = false,
                    IsApproved = false,
                    PointsBias = dto.PointsBias,
                    NormalizedUserName = (dto.Email ?? dto.LastName).ToUpper()
                };

                context.Users.Add(newTeacher);
            }

            await context.SaveChangesAsync();
            OnChanged?.Invoke();
        }
        public async Task<List<Course>> GetCoursesForTeacherAsync(Teacher teacher)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Courses
                .Where(c => c.Teacher == teacher) // Nur meine Kurse!
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Person)
                .ToListAsync();
        }

        /*
         * Rulesets Saver
         */
        public async Task SaveRuleSetAsync(RuleSet incomingSet)
        {
            using var context = _dbFactory.CreateDbContext();

            // Wir laden das Set INKLUSIVE der vorhandenen Regeln aus der DB
            var dbSet = await context.RuleSets
                .Include(s => s.Rules)
                .FirstOrDefaultAsync(s => s.Id == incomingSet.Id);

            if (dbSet == null)
            {
                // Neues Set anlegen
                incomingSet.SchoolId = context.GetSchoolId(); // WICHTIG: SchoolId setzen für den Filter!
                context.RuleSets.Add(incomingSet);
            }
            else
            {
                // 1. Stammdaten (Titel etc.) aktualisieren
                context.Entry(dbSet).CurrentValues.SetValues(incomingSet);

                // 2. Regeln abgleichen
                // Lösche Regeln, die im incomingSet nicht mehr drin sind
                foreach (var dbRule in dbSet.Rules.ToList())
                {
                    if (!incomingSet.Rules.Any(r => r.Id == dbRule.Id))
                        context.Rules.Remove(dbRule);
                }
                foreach (var incomingRule in incomingSet.Rules)
                {
                    var dbRule = dbSet.Rules.FirstOrDefault(r => r.Id == incomingRule.Id);
                    if (dbRule == null)
                    {
                        incomingRule.RuleSetId = dbSet.Id;
                        context.Entry(incomingRule).State = EntityState.Added;
                        dbSet.Rules.Add(incomingRule);
                    }
                    else
                    {
                        context.Entry(dbRule).CurrentValues.SetValues(incomingRule);
                    }
                }
            }
            try
            {
                await context.SaveChangesAsync();
                OnChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }


        public async Task<List<RuleSet>> GetRuleSetsAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.RuleSets.Include(s => s.Rules).ToListAsync(); // Nutze await und ToListAsync für bessere Performance

        }
        public async Task<Course?> GetCourseByIdAsync(Guid id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Courses
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Person)
                .Include(c => c.SeatLayouts!)         // ✅ NEU
            .ThenInclude(l => l.Seats!)         // ✅ NEU
                .ThenInclude(s => s.Participant) // ✅ NEU
                    .ThenInclude(p => p!.Person)  // ✅ NEU
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<RuleSet?> GetRuleSetByIdAsync(Guid id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.RuleSets
                .Include(s => s.Rules)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task SaveOccurrenceAsync(RuleOccurrence occurrence)
        {
            using var context = _dbFactory.CreateDbContext();
            context.RuleOccurrences.Add(occurrence);
            await context.SaveChangesAsync();
        }

        public async Task DeleteStudentAsync(Guid Id)
        {
            using var context = _dbFactory.CreateDbContext();
            var student = await context.Students.FindAsync(Id);
            if (student != null)
            {
                context.Students.Remove(student);
                await context.SaveChangesAsync();
                OnChanged?.Invoke();
            }
        }
        public async Task<List<RuleOccurrence>> GetTodayOccurrencesAsync(Guid courseId)
        {
            using var context = _dbFactory.CreateDbContext();

            // Wir holen den Start des heutigen Tages
            var today = DateTime.Today;

            return await context.RuleOccurrences
                .Where(o => o.CourseId == courseId && o.Timestamp >= today)
                .ToListAsync();
        }
        public async Task<List<RuleOccurrence>> GetLastOccurrencesAsync(Guid personId, int count)
        {
            Console.WriteLine($"GetLastOccurenceAsync AUFRUF mit ID {personId}.");
            using var context = _dbFactory.CreateDbContext();
            return await context.RuleOccurrences
                .Where(o => o.PersonId == personId)
                .OrderByDescending(o => o.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<RuleOccurrence>> GetOccurrencesByDateAsync(Guid personId, DateTime start, DateTime end)
        {
            var endOfDay = end.Date.AddDays(1).AddTicks(-1); //Janse Daach!
            using var context = _dbFactory.CreateDbContext();
            return await context.RuleOccurrences
                .Where(o => o.PersonId == personId)
                .Where(o => o.Timestamp >= start.Date && o.Timestamp <= endOfDay)
                .OrderByDescending(o => o.Timestamp)
                .ToListAsync();
        }

        /*
         * 
         * Schulen: Hier müssen wir den Tenant-Türsteher umgehen, 
         * damit wir alle Schwulen sehen können, 
         * auch die von anderen Tenants. Das ist wichtig
         * 
         */
        public async Task<List<School>> GetAllSchoolsGlobalAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            return await context.Schools
                .IgnoreQueryFilters() // <--- Schaltet den Tenant-Türsteher aus
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        public async Task<School> GetSchoolWithIdAsync(Guid SchoolId)
        {
            using var context = _dbFactory.CreateDbContext();

            return await context.Schools.FirstOrDefaultAsync(s => s.Id == SchoolId);
        }

        public async Task<bool> IsSchoolNameTakenAsync(string name)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Schools
                .IgnoreQueryFilters()
                .AnyAsync(s => s.Name.ToLower() == name.ToLower());
        }

        public async Task<School> CreateSchoolAsync_Obsolete(string name, string address)
        {
            using var context = _dbFactory.CreateDbContext();
            var school = new School
            {
                Id = Guid.NewGuid(),
                Name = name,
                Address = address,
                City = "Unbekannt",
                Email = "info@schule.de",   // <--- Und das
                AccessCode = "START2024",
                GlobalRuleAverage = 0.0
            };
            context.Schools.Add(school);
            await context.SaveChangesAsync();
            return school;
        }

        public async Task<School> CreateSchoolAsync(Guid teacherId, string name, string address)
        {
            using var _context = _dbFactory.CreateDbContext();
            // 1. Neue Schule anlegen
            var school = new School
            {
                Id = Guid.NewGuid(),
                Name = name,
                Address = address,
                City = "Unbekannt",
                Email = "info@schule.de",   // <--- Und das
                AccessCode = "START2024",
                GlobalRuleAverage = 0
            };
            _context.Schools.Add(school);

            // 2. Den Gründer zum Admin befördern
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Id == teacherId);
            if (teacher != null)
            {
                teacher.SchoolId = school.Id;
                teacher.ActiveSchoolId = school.Id;
                teacher.IsApproved = true;
                teacher.IsAdmin = true; // LORD STATUS
                _context.Users.Update(teacher);
            }

            await _context.SaveChangesAsync();
            return school;
        }
        public async Task ToggleAssignmentStatusAsync(Guid assignmentId, Guid courseParticipantId)
        {
            using var _context = _dbFactory.CreateDbContext();
            var status = await _context.StudentAssignmentStatuses
                .FirstOrDefaultAsync(s => s.CourseAssignmentId == assignmentId
                                       && s.CourseParticipantId == courseParticipantId);

            if (status == null)
            {
                // 2. Erstmals abhaken: Neuen Eintrag erstellen
                var newStatus = new StudentAssignmentStatus
                {
                    CourseAssignmentId = assignmentId,
                    CourseParticipantId = courseParticipantId,
                    IsCompleted = true,
                    CompletedAt = DateTime.Now
                };
                _context.StudentAssignmentStatuses.Add(newStatus);
            }
            else
            {
                // 3. Status umschalten (Toggle)
                status.IsCompleted = !status.IsCompleted;
                status.CompletedAt = status.IsCompleted ? DateTime.Now : null;
                _context.StudentAssignmentStatuses.Update(status);
            }

            await _context.SaveChangesAsync();
        }
        public async Task MarkAllAsCompletedAsync(Guid assignmentId, Guid courseId)
        {
            using var _context = _dbFactory.CreateDbContext();
            var participants = await _context.CourseParticipants
                .Where(p => p.CourseId == courseId)
                .ToListAsync();

            foreach (var p in participants)
            {
                // Wir nutzen die Toggle-Logik oder setzen es direkt
                var status = await _context.StudentAssignmentStatuses
                    .FirstOrDefaultAsync(s => s.CourseAssignmentId == assignmentId && s.CourseParticipantId == p.Id);

                if (status == null)
                {
                    _context.StudentAssignmentStatuses.Add(new StudentAssignmentStatus
                    {
                        CourseAssignmentId = assignmentId,
                        CourseParticipantId = p.Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now
                    });
                }
                else
                {
                    status.IsCompleted = true;
                    status.CompletedAt = DateTime.Now;
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<CourseAssignment> CreateAssignmentAsync(CourseAssignment assignment)
        {
            using var _context = _dbFactory.CreateDbContext();
            assignment.SchoolId = _context.GetSchoolId();

            // Zeitstempel setzen, falls noch nicht geschehen
            if (assignment.CreatedAt == default)
            {
                assignment.CreatedAt = DateTime.Now;
            }

            _context.CourseAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return assignment;
        }

        public async Task<List<CourseAssignment>> GetAssignmentsForCourseAsync(Guid courseId)
        {
            using var _context = _dbFactory.CreateDbContext();
            return await _context.CourseAssignments
                .Where(a => a.CourseId == courseId)
                .Include(a => a.StatusEntries) // Lädt die "Häkchen" (Status) der Schüler direkt mit
                .OrderByDescending(a => a.CreatedAt) // Neueste Aufgaben zuerst
                .ToListAsync();
        }

        public async Task DeleteAssignmentAsync(Guid assignmentId)
        {
            using var _context = _dbFactory.CreateDbContext();
            var assignment = await _context.CourseAssignments
                .Include(a => a.StatusEntries) // Die Verknüpfungen mitladen
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
            if (assignment == null)
            {
                return;
            }
            if (assignment.StatusEntries != null && assignment.StatusEntries.Any())
            {
                _context.StudentAssignmentStatuses.RemoveRange(assignment.StatusEntries);
            }
            _context.CourseAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
        public async Task SyncAllSchoolsGlobalAverageAsync()
        {
            using var context = _dbFactory.CreateDbContext();

            // 1. Berechne den Durchschnitt der PointsBias aller Lehrer pro Schule
            // Wir filtern Lehrer ohne SchoolId direkt raus
            var biasPerSchool = await context.Users // Deine Teacher-Tabelle
                .Where(t => t.SchoolId != null)
                .GroupBy(t => t.SchoolId)
                .Select(g => new
                {
                    SchoolId = g.Key,
                    AverageBias = g.Average(t => t.PointsBias)
                })
                .ToListAsync();

            // 2. Alle Schulen laden, die ein Update benötigen
            var schoolIds = biasPerSchool.Select(x => x.SchoolId).ToList();
            var schools = await context.Schools
                .Where(s => schoolIds.Contains(s.Id))
                .ToListAsync();

            // 3. Werte zuordnen
            foreach (var school in schools)
            {
                var calculation = biasPerSchool.First(x => x.SchoolId == school.Id);
                school.GlobalRuleAverage = calculation.AverageBias;

                // Optional: Falls du es explizit markieren willst (meist nicht nötig bei Tracked Entities)
                context.Entry(school).State = EntityState.Modified;
            }

            // 4. Einmal speichern für ALLES
            var affected = await context.SaveChangesAsync();
            Console.WriteLine($"[MAINTENANCE] {affected} Schulen mit neuem GlobalRuleAverage aktualisiert.");
        }
    }
    }