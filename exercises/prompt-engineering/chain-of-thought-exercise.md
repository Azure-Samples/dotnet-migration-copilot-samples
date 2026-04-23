# Chain-of-Thought Prompting Exercise

## Objective
Test how asking for step-by-step reasoning before code generation improves output quality for complex features.

## Exercise

### Task
Build a "Department Comparison" feature that computes and displays analytical metrics.

### Prompt to Use
```
I want to build a "Department Comparison" feature for Contoso University.
Before writing any code, think through this step by step:

1. ANALYZE THE DATA MODEL:
   What entities and relationships are available?
   - Department has Courses
   - Course has Enrollments
   - Enrollment has Student and Grade
   - Department has Budget and Administrator (Instructor)
   Trace through the entity relationships and determine what's queryable.

2. DETERMINE USEFUL METRICS:
   What department-level metrics can we compute from this data?
   Consider: total students, total courses, average grade, budget per student,
   student-to-course ratio, grade distribution, etc.
   Which are meaningful? Which might have edge cases (e.g., division by zero)?

3. DESIGN THE DATA FLOW:
   What LINQ queries are needed? Should they be separate queries or a single
   projected query? What ViewModel should hold the results?
   Think about performance — avoid N+1 queries.

4. PLAN THE UI:
   How should the comparison be displayed? Consider a table with departments
   as rows and metrics as columns. Should any metrics be highlighted or
   color-coded? How do we handle departments with no enrollments?

5. IMPLEMENT:
   Only after completing steps 1-4, write the complete implementation:
   ViewModel, Controller action, Razor view.

Show me your reasoning at each step before writing the code.
```

### Evaluation Criteria

1. **Reasoning quality**: Did Copilot identify edge cases (empty departments, null grades)?
2. **Query efficiency**: Did the reasoning lead to a single optimized query instead of N+1?
3. **ViewModel design**: Is it well-structured for the view's needs?
4. **Error handling**: Did the reasoning step catch division-by-zero for averages?
5. **UI decisions**: Did it make good choices about formatting numbers, handling missing data?

### What to Note
- Compare the code quality with what you'd get from a direct "build a department comparison" prompt
- Did the reasoning steps change the final implementation in meaningful ways?
- Were there any reasoning steps where Copilot's analysis was wrong?
