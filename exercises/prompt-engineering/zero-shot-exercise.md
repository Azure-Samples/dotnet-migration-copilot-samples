# Zero-Shot Prompting Exercise

## Objective
Test Copilot's ability to generate working code without providing any examples or reference patterns.

## Exercise

### Task
Create a global search feature for Contoso University that searches across multiple entities.

### Prompt to Use (copy-paste into Copilot Chat)
```
Add a global search endpoint to the Contoso University application.

Create a SearchController with an Index action that accepts a "query" string parameter.
It should search across:
- Students (by first name or last name)
- Courses (by title)
- Instructors (by first name or last name)
- Departments (by name)

Return all matching results grouped by entity type in a single view.
```

### Evaluation Criteria
After Copilot generates the code, check:

1. **Does it inherit from BaseController?** (project convention)
2. **Does it use EF Core `.Include()` properly?** (required for related data)
3. **Does the search handle empty/null query strings?**
4. **Does it create a proper ViewModel?** (needed to combine multiple entity types)
5. **Does the view use Bootstrap styling?** (project convention)
6. **Is the search case-insensitive?** (`.Contains()` in EF Core with SQL Server is case-insensitive by default, but check)
7. **Are there any N+1 query issues?**

### What to Note
- Record what the zero-shot output got right and wrong
- You will compare this with the one-shot exercise output next
