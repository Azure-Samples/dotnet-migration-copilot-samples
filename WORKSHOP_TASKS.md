# GitHub Copilot Workshop — Contoso University (.NET)

> Adapted from the [GitHubCopilotWorkshop](https://github.com/avivka/GitHubCopilotWorkshop) for the **Contoso University** ASP.NET MVC 5 application (.NET Framework 4.8, Entity Framework Core 3.1).

---

## Task 0 — Setup & Model Selection (15 min)

### Goal
Set up your development environment and configure AI model selection.

### Steps

1. **Open the solution** in Visual Studio (2019 or later) or VS Code with the C# Dev Kit extension.
2. **Restore NuGet packages** and build the solution to ensure everything compiles.
3. **Verify prerequisites**:
   - SQL Server LocalDB is installed and running
   - MSMQ is enabled (Windows Feature → Microsoft Message Queue Server)
4. **Run the application** via IIS Express and confirm the homepage loads at `https://localhost:<port>/`.
5. **Install GitHub Copilot** (and Copilot Chat) extensions if not already installed.
6. **Select your model** in Copilot Chat settings:
   - GPT-4.1 for general development tasks
   - Claude Sonnet 4.5 for optimization and refactoring tasks
7. **Explore the database** — navigate to Students, Courses, Instructors, and Departments pages to see the seeded data.

---

## Task 1 — Core Copilot Basics (45 min)

### 1.1 Repository Exploration with @workspace

Use the `@workspace` chat participant to understand the Contoso University architecture.

**Try these prompts:**

- `@workspace What is the overall architecture of this application? What framework and ORM does it use?`
- `@workspace Describe the data model. What entities exist and how are they related?`
- `@workspace How does the notification system work? Trace the flow from a controller action to MSMQ.`
- `@workspace What inheritance strategy is used for Student and Instructor? Where is it configured?`
- `@workspace List all controllers and their CRUD actions. Which ones support pagination or search?`

**What to observe:** How Copilot navigates across controllers, models, views, and configuration files to give you a holistic picture.

---

### 1.2 Custom Instructions

Create a `.github/copilot-instructions.md` file at the repository root with project-specific conventions.

**Suggested content:**

```markdown
# Contoso University — Copilot Instructions

## Tech Stack
- ASP.NET MVC 5 on .NET Framework 4.8
- Entity Framework Core 3.1.32 with SQL Server LocalDB
- Razor views with Bootstrap 5.3.3 and jQuery 3.7.1

## Coding Conventions
- All controllers inherit from `BaseController` (provides `db` context and `SendEntityNotification`)
- Use `[Bind(Include = "...")]` on POST actions to prevent over-posting
- Use `[ValidateAntiForgeryToken]` on all POST actions
- ViewBag is used for passing dropdown data (e.g., `ViewBag.DepartmentID`)
- Pagination uses the `PaginatedList<T>` helper class (page size = 10)
- Send MSMQ notifications for CREATE, UPDATE, DELETE operations via `SendEntityNotification()`

## Patterns to Follow
- Follow existing controller patterns (see `StudentsController` as the reference implementation)
- Use EF Core `.Include()` / `.ThenInclude()` for eager loading relationships
- Validate DateTime fields for SQL Server `datetime2` range (1753–9999)
- For file uploads, validate extension and size, generate unique filenames with GUID
```

**Test it:** After saving, ask Copilot to generate a new controller action. Notice how it follows the conventions (e.g., uses `BaseController`, adds `[ValidateAntiForgeryToken]`, calls `SendEntityNotification`).

---

### 1.3 Code Review with Copilot

Select code in the following files and use the **"Review and Comment"** feature (or ask Copilot Chat to review):

1. **`Controllers/StudentsController.cs`** — Review the `Details` action. Does `.Single()` handle the case where no student is found? (Hint: it throws instead of returning null, making the `if (student == null)` check unreachable.)

2. **`Controllers/CoursesController.cs`** — Review the file upload logic in the `Create` POST action. Is there any issue with the duplicated validation code between `Create` and `Edit`?

3. **`Controllers/DepartmentsController.cs`** — Review the concurrency handling in the `Edit` action. Is the optimistic concurrency pattern implemented correctly?

4. **`Services/LoggingService.cs`** — This file is empty. Ask Copilot what a proper logging service should contain for this application.

**Prompt example:**
```
Review this code for potential bugs, security issues, and improvements.
Focus on null reference risks, exception handling, and .NET best practices.
```

---

### 1.4 Add Documentation with /doc

Select the following code blocks and use the inline `/doc` command to generate XML documentation comments:

1. **`Models/Person.cs`** — Document the base class and its properties
2. **`Models/Course.cs`** — Document the entity including its relationships
3. **`Data/SchoolContext.cs`** — Document the DbContext and its `OnModelCreating` configuration
4. **`Controllers/StudentsController.cs`** — Document the `Index` action (explain sorting, filtering, pagination parameters)
5. **`PaginatedList.cs`** — Document the generic pagination helper

**What to observe:** How Copilot generates appropriate `<summary>`, `<param>`, and `<returns>` tags following .NET XML documentation conventions.

---

### 1.5 Generate Unit Tests with /tests

Use the `/tests` command to generate unit tests for the application:

1. **Select `Controllers/StudentsController.cs`** and generate tests.
   - The generated tests should cover: Index (with sorting/filtering/pagination), Details, Create (GET & POST), Edit (GET & POST), Delete.
   - You will likely need to mock `SchoolContext` and set up in-memory data.

2. **Select `PaginatedList.cs`** and generate tests.
   - Test edge cases: empty list, single page, multiple pages, boundary page numbers.

3. **Select `Data/DbInitializer.cs`** and generate tests.
   - Verify seed data is created correctly and idempotent (doesn't duplicate on re-run).

4. **Run the generated tests.** If any fail, use the `/fix` command to correct them.

**Tip:** You may need to add a test project to the solution first. Ask Copilot:
```
@workspace Help me create an MSTest or xUnit test project for this solution.
What NuGet packages do I need for mocking the EF Core DbContext?
```

---

### 1.6 Fix a Bug

There are several latent issues in the codebase. Pick one (or more) to investigate and fix:

**Bug 1 — Unreachable Null Check:**
In `StudentsController.Details()` (line 72), `.Single()` will throw `InvalidOperationException` if no student is found — the `if (student == null)` check on line 73 is never reached. The same pattern exists in `CoursesController.Details()` and `CoursesController.Delete()`.

> **Task:** Use Copilot to identify and fix all occurrences. Replace `.Single()` with `.SingleOrDefault()` (or `.FirstOrDefault()`) so the null check works properly.

**Bug 2 — Empty Logging Service:**
`Services/LoggingService.cs` is an empty stub. The application uses `Trace.TraceError()` and `Debug.WriteLine()` scattered across controllers.

> **Task:** Ask Copilot to implement a proper `LoggingService` that centralizes logging, and refactor the controllers to use it.

**Bug 3 — Duplicated File Upload Validation:**
The file upload validation logic in `CoursesController` is copy-pasted between `Create` and `Edit` (lines 54–95 and 136–188).

> **Task:** Ask Copilot to extract this into a reusable private method and call it from both actions.

---

### 1.7 Create a New Feature — Course Enrollment Report

Build a new page that shows enrollment statistics per course.

**Requirements:**
- Add a new action `EnrollmentReport` to `HomeController` (or create a `ReportsController`)
- Query all courses with their enrollment counts and average grades
- Display a table showing: Course Title, Department, Number of Students, Average Grade
- Add sorting by any column
- Add a link to this page in the navigation bar (`_Layout.cshtml`)

**Prompt example for agent mode:**
```
Create a new "Enrollment Report" feature for the Contoso University application.

Add a ReportsController with an EnrollmentReport action that:
1. Queries all courses with enrollment counts and average grades
2. Returns a view showing a sortable table with columns: Course Title, Department, # Students, Average Grade
3. Follow the existing controller patterns (inherit BaseController, use EF Core Include)
4. Create the Razor view with Bootstrap table styling matching the existing pages
5. Add a "Reports" link in the navigation bar in Views/Shared/_Layout.cshtml
```

---

## Task 2 — Prompt Engineering (45 min)

### 2.1 Zero-Shot Prompting

Create a search feature without providing any examples or patterns.

**Prompt:**
```
Add a global search endpoint to the Contoso University application.
Create a SearchController with an Index action that accepts a query string parameter.
It should search across Students (by name), Courses (by title),
and Instructors (by name), and return all matching results in a single view.
```

**Observe:** Does Copilot produce working code on its own? Does it follow the project's conventions (BaseController, EF Core includes, Bootstrap styling)? What's missing or incorrect?

---

### 2.2 One-Shot Prompting

Use the existing `StudentsController` as an example pattern to generate a new feature.

**Prompt:**
```
I need to add a full CRUD feature for managing "Scholarships" in the Contoso University app.

Here is the pattern to follow — use StudentsController as the reference:
- File: Controllers/StudentsController.cs (existing controller with Index, Details, Create, Edit, Delete)
- It inherits from BaseController
- Uses [ValidateAntiForgeryToken] and [Bind] attributes
- Sends notifications via SendEntityNotification()
- Index has sorting, filtering, and pagination

A Scholarship has: ScholarshipID (int, PK), Name (string, required, max 100),
Amount (decimal, required), AwardDate (DateTime), StudentID (FK to Student).

Generate: Model, Controller, all 5 Views (Index, Details, Create, Edit, Delete),
DbSet in SchoolContext, and navigation link in _Layout.cshtml.
Follow the EXACT same patterns as StudentsController.
```

**Compare:** How does the one-shot output quality compare to the zero-shot approach? Does providing the reference implementation help Copilot follow project conventions more accurately?

---

### 2.3 Chain-of-Thought Prompting

Use step-by-step reasoning for a complex analytical feature.

**Prompt:**
```
I want to build a "Department Comparison" feature. Before writing code,
think through this step by step:

1. First, analyze what data is available: departments have courses,
   courses have enrollments, enrollments have grades and students.
2. Then, determine what metrics would be useful to compare departments:
   total students, total courses, average grade, budget per student.
3. Next, design the data flow: what LINQ queries are needed,
   what ViewModel should hold the results.
4. Then, plan the UI: a comparison table with departments as columns
   and metrics as rows.
5. Finally, implement the complete feature.

Show me your reasoning at each step before writing the code.
```

**Observe:** Does the chain-of-thought approach produce a more thoughtful, well-structured implementation? Does the reasoning help catch edge cases (e.g., departments with no enrollments, division by zero for averages)?

---

### 2.4 Prompt Files for Larger Tasks

Create reusable prompt templates for common Contoso University development patterns.

**Create `exercises/prompt-engineering/controller-generator.prompt.md`:**
```markdown
# Controller Generator for Contoso University

## Context
This is an ASP.NET MVC 5 application using EF Core 3.1 with SQL Server.
All controllers inherit from BaseController which provides:
- `db` (SchoolContext) for database access
- `SendEntityNotification(entityType, entityId, displayName, operation)` for MSMQ notifications

## Template Variables
- {{EntityName}}: The entity class name (e.g., "Scholarship")
- {{Properties}}: The entity properties list
- {{RelatedEntities}}: Foreign keys and navigation properties

## Requirements
1. Inherit from BaseController
2. Implement full CRUD (Index, Details, Create GET/POST, Edit GET/POST, Delete GET/POST)
3. Use [ValidateAntiForgeryToken] on all POST actions
4. Use [Bind(Include = "...")] to prevent over-posting
5. Call SendEntityNotification() on Create, Edit, Delete
6. Include proper null checks and error handling
7. Use .Include() for eager loading related entities

## Generate
A complete controller for {{EntityName}} following all conventions above.
```

**Create `exercises/prompt-engineering/view-generator.prompt.md`:**
```markdown
# View Generator for Contoso University

## Context
Views use Razor syntax with Bootstrap 5.3.3 styling.
Follow the existing view patterns in Views/Students/ as reference.

## Template Variables
- {{EntityName}}: The entity to generate views for
- {{Properties}}: Columns to display

## Requirements
1. Index view with HTML table, Bootstrap classes, Create/Edit/Details/Delete links
2. Details view showing all properties with display names
3. Create view with form, validation summaries, and anti-forgery token
4. Edit view similar to Create but pre-populated
5. Delete view with confirmation
6. All views use @model directive and @Html helpers
```

**Use the templates:** Attach both prompt files in Copilot Chat and ask it to generate a complete "Scholarship" feature using the templates.

---

### 2.5 Prompt Refinement and Rollback

Iterate on generating a Student Dashboard component.

**Round 1 — Initial prompt:**
```
Create a Student Dashboard view that shows a student's enrolled courses,
grades, and GPA.
```

**Round 2 — Refine by editing the prompt (don't start a new chat):**
```
Update the Student Dashboard to also include:
- A progress bar showing credits completed vs. required (assume 120 credits required)
- Color-coded grades (A=green, B=blue, C=yellow, D=orange, F=red)
- A chart showing grade distribution (use a simple HTML/CSS bar chart, no JS libraries)
```

**Round 3 — Switch models and compare:**
- Regenerate with GPT-4.1 and note the output
- Switch to Claude Sonnet 4.5 and regenerate
- Compare: Which model produces cleaner Razor syntax? Better Bootstrap integration? More accurate EF Core queries?

**Round 4 — Rollback and try a different approach:**
- Use Copilot's undo/rollback to go back to Round 1
- Try a completely different prompt angle:
  ```
  Looking at the existing Student Details view and the Enrollment model,
  enhance the Details view to include GPA calculation and a visual grade summary.
  ```

---

## Task 3 — Agents (45 min)

### 3.1 Built-in Chat Participants

Experiment with the built-in chat participants in the context of the Contoso University app:

**@workspace:**
- `@workspace How is the Person inheritance hierarchy mapped in EF Core? Show me the relevant configuration.`
- `@workspace What would break if I renamed the "Person" table to "People"?`
- `@workspace Find all places where notifications are sent. Is any entity missing notification support?`

**@vscode:**
- `@vscode How do I set up debugging for this ASP.NET MVC project?`
- `@vscode What extensions would help with .NET Framework 4.8 development?`
- `@vscode How do I view the SQL Server LocalDB database?`

**@terminal:**
- `@terminal How do I run EF Core migrations for this project?`
- `@terminal How do I check if MSMQ is running on my machine?`
- `@terminal Show me how to build and run this project from the command line.`

---

### 3.2 Study an Existing Agent

Create and study a code-reviewer agent for .NET applications.

**Create `exercises/agent-creation/dotnet-code-reviewer.agent.md`:**
```markdown
# .NET Code Reviewer Agent

## Role
You are a senior .NET developer specializing in ASP.NET MVC and Entity Framework.
You review code for correctness, security, performance, and adherence to .NET best practices.

## Review Categories
1. **Security**: SQL injection, XSS, CSRF, over-posting, path traversal
2. **Performance**: N+1 queries, missing indexes, unnecessary eager loading, large result sets
3. **Correctness**: Null references, exception handling, concurrency issues, data validation
4. **Maintainability**: DRY violations, naming conventions, separation of concerns

## Severity Levels
- CRITICAL: Security vulnerabilities or data loss risks
- WARNING: Performance issues or potential runtime errors
- INFO: Style improvements or minor refactoring opportunities

## Output Format
For each finding:
- **File**: path/to/file.cs
- **Line(s)**: line number(s)
- **Severity**: CRITICAL | WARNING | INFO
- **Category**: Security | Performance | Correctness | Maintainability
- **Finding**: Description of the issue
- **Recommendation**: How to fix it with code example
```

**Test it:** Use the agent to review `Controllers/CoursesController.cs`. Does it catch the duplicated upload logic? The `.Single()` vs `.SingleOrDefault()` issue?

---

### 3.3 Create Your Own Agent

Build a migration-readiness analyzer agent for assessing .NET Framework to .NET 8+ migration.

**Create `exercises/agent-creation/migration-analyzer.agent.md`:**
```markdown
# .NET Migration Analyzer Agent

## Role
You analyze .NET Framework 4.8 applications for migration readiness to .NET 8+.

## Analysis Categories
1. **Breaking Changes**: APIs removed or changed in .NET 8 (System.Web, MSMQ, etc.)
2. **Dependencies**: NuGet packages that need updating or replacing
3. **Architecture**: Patterns that need reworking (Global.asax → Program.cs, Web.config → appsettings.json)
4. **EF Core**: Already using EF Core 3.1 — what changes for EF Core 8?
5. **Frontend**: Razor views, bundling, Bootstrap/jQuery compatibility

## Severity Levels
- BLOCKER: Must be resolved before migration (e.g., System.Web.Mvc → Microsoft.AspNetCore.Mvc)
- SIGNIFICANT: Major rework required (e.g., MSMQ → Azure Service Bus or RabbitMQ)
- MINOR: Small code changes needed (e.g., API signature changes)
- COMPATIBLE: Already works or trivial to migrate

## Output Format
Migration Report with:
- Executive Summary (overall readiness score)
- Detailed findings per category
- Recommended migration order
- Estimated effort per component
```

**Test it:** Run the agent against the entire Contoso University codebase and review the migration report.

---

### 3.4 Agent Mode Feature Building

Switch to **Agent Mode** and build a complete new feature end-to-end.

**Feature: Student Enrollment Management**

```
Using agent mode, build a complete "Enrollment Management" feature for Contoso University:

1. Create an EnrollmentsController that allows:
   - View all enrollments with student name, course title, and grade
   - Filter enrollments by course, student, or grade
   - Create new enrollments (select student + course from dropdowns)
   - Edit enrollment grades
   - Delete enrollments

2. Create all necessary Razor views following existing patterns:
   - Index with sortable/filterable table
   - Create with dropdowns for Student and Course
   - Edit for updating grades
   - Delete with confirmation
   - Details showing full enrollment info

3. Add proper navigation in _Layout.cshtml

4. Follow ALL existing conventions:
   - Inherit BaseController
   - Send notifications on CRUD operations
   - Use [ValidateAntiForgeryToken]
   - Use Bootstrap 5 table styling
   - Include proper error handling

Build and verify each file compiles correctly.
```

**Observe:** How does agent mode handle the multi-file generation? Does it discover and follow existing patterns automatically? Does it wire everything up correctly (routes, navigation, DbContext)?

---

### 3.5 Vision — Generate View from Design

Take a screenshot or create a mockup of a "Student Profile Card" design, then use Copilot's vision capabilities.

**Steps:**
1. Find or sketch a student profile card design (name, photo placeholder, enrollment date, GPA, list of courses)
2. Attach the image to Copilot Chat
3. **Prompt:**
   ```
   Generate a Razor partial view (_StudentCard.cshtml) that matches this design.
   Use Bootstrap 5.3.3 classes for styling.
   The model should be a Student entity with included Enrollments and Courses.
   Include a calculated GPA display based on the Grade enum values.
   ```
4. Integrate the generated partial view into the Students/Details page

---

## Task 4 — Plan Mode & Code Review (30 min)

### 4.1 Use Plan Mode for a Complex Feature

Switch to **Plan Mode** to architect a larger feature before writing code.

**Feature: Course Prerequisites System**

**Prompt:**
```
Plan mode: I want to add a course prerequisites system to Contoso University.

Requirements:
- Courses can have zero or more prerequisite courses
- A student cannot enroll in a course unless they have passed (grade C or above)
  all prerequisites
- The UI should show prerequisites on the Course Details page
- When creating an enrollment, validate prerequisites are met
- Show a clear error message listing unmet prerequisites

Plan the implementation: data model changes, EF Core configuration,
controller modifications, view updates, and validation logic.
```

**What to observe:** How does Plan Mode help you think through the data model (self-referencing many-to-many on Course), the validation logic, and the UX before writing any code?

---

### 4.2 Code Review with Copilot

Use Copilot to perform a comprehensive code review of the entire application.

**Prompt:**
```
Review the Contoso University codebase for:

1. Security vulnerabilities (OWASP Top 10)
2. Entity Framework anti-patterns (N+1 queries, missing AsNoTracking for read-only)
3. Error handling gaps (unhandled exceptions, missing try-catch)
4. Accessibility issues in the Razor views
5. Performance concerns (missing pagination, unbounded queries)

Provide a prioritized list of findings with specific file/line references
and recommended fixes.
```

---

## Bonus Challenges

### Bonus 1 — Implement the LoggingService
The `Services/LoggingService.cs` file is empty. Implement a proper logging service that:
- Logs to both file and console
- Supports log levels (Debug, Info, Warning, Error)
- Includes timestamp, controller name, and action name
- Replace all `Trace.TraceError()` and `Debug.WriteLine()` calls in the controllers

### Bonus 2 — Add Data Export
Add a feature to export Student or Course data to CSV format:
- Add "Export to CSV" buttons on the Students and Courses Index pages
- Implement CSV generation in the controllers
- Properly handle special characters and encoding

### Bonus 3 — Add Client-Side Validation
The application uses server-side validation only for many fields. Add unobtrusive client-side validation for:
- Student enrollment date (must be in the past or today)
- Course credits (0–5 range with visual feedback)
- Department budget (positive numbers only, currency formatting)

### Bonus 4 — Create an API Controller
Add a Web API–style controller (`ApiStudentsController`) that returns JSON:
- `GET /api/students` — paginated list of students
- `GET /api/students/{id}` — single student with enrollments
- `POST /api/students` — create a new student
- Use content negotiation to return JSON
- Add basic input validation

---

## Tips for Participants

1. **Be specific in prompts** — reference file names, line numbers, and existing patterns
2. **Use @workspace first** — understand the codebase before making changes
3. **Follow existing patterns** — point Copilot to `StudentsController` as the canonical example
4. **Iterate** — refine prompts based on initial output quality
5. **Compare models** — try the same prompt with different AI models and observe differences
6. **Review generated code** — never blindly accept; check for security issues, null refs, and convention violations
7. **Use agent mode for multi-file changes** — it handles cross-file dependencies better than inline suggestions
