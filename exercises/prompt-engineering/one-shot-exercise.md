# One-Shot Prompting Exercise

## Objective
Test how providing a single reference example improves Copilot's output quality.

## Exercise

### Task
Create the same global search feature from the zero-shot exercise, but this time provide the StudentsController as a reference pattern.

### Prompt to Use
```
I need to add a global search feature to the Contoso University application.

Here is the pattern I want you to follow. Use StudentsController as the reference:

File: Controllers/StudentsController.cs
Key patterns to replicate:
- Inherits from BaseController (provides `db` context)
- Uses ViewBag for passing state to views (ViewBag.CurrentFilter, ViewBag.CurrentSort)
- Filters using .Where() with .Contains() for string search
- Returns View with data
- Views use Bootstrap 5 table styling with class="table table-striped"

Now create a SearchController with:
- An Index action that accepts a "query" parameter
- Searches Students (by name), Courses (by title), Instructors (by name), Departments (by name)
- A SearchViewModel to hold results grouped by entity type
- A Razor view with Bootstrap 5 styling matching the existing pages
- A search box in the navigation bar (_Layout.cshtml) that submits to this controller
```

### Evaluation Criteria
Compare the one-shot output with the zero-shot output:

1. **Convention adherence**: Does the one-shot version follow project patterns better?
2. **Code structure**: Is the ViewModel better designed?
3. **View quality**: Does the view match existing styling more closely?
4. **Completeness**: Does it include navigation integration?
5. **Error handling**: Does providing context improve robustness?

### What to Note
- Specifically document what improved vs. the zero-shot version
- Did the reference example cause any "over-fitting" (copying irrelevant patterns like pagination when not needed)?
