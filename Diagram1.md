flowchart TD
    A["1. Neues Model\n CourseAppointment.cs"] --> B["2. Course.cs\n Day/StartTime/EndTime → List&lt;CourseAppointment&gt;"]
    B --> C["3. AppDataContext.cs\n DbSet hinzufügen"]
    C --> D["4. EF Migration\n add-migration + Datenmigration"]
    D --> E["5. DataService.cs\n Include + Sync erweitern"]
    E --> F["6. Timetable.razor\n c.Appointments iterieren"]
    E --> G["7. ManageCourses.razor\n Termin-Liste bearbeiten"]
    E --> H["8. SvgCanvas.razor\n Kurs-Auswahl anpassen"]
```