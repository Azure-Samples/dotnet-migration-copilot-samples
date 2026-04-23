# Exercise: Study an Existing Agent

## Objective
Understand how Copilot agents are structured by analyzing the `.NET Code Reviewer` agent definition.

## Steps

### 1. Read the Agent Definition
Open `exercises/agent-creation/dotnet-code-reviewer.agent.md` and identify:

- **Role definition**: How does the agent introduce its expertise?
- **Context section**: What project-specific information is provided?
- **Review categories**: How are the analysis areas organized?
- **Severity levels**: How are findings prioritized?
- **Output format**: What structure does the agent follow when reporting?

### 2. Analyze the Structure

Answer these questions:
1. Why does the agent need to know the tech stack (ASP.NET MVC 5, EF Core 3.1)?
2. How do the review categories map to real-world code quality concerns?
3. Why are severity levels important? How would you use them to prioritize fixes?
4. What makes the output format useful for developers? Could it be improved?

### 3. Test the Agent

Use the code-reviewer agent to review the following files:

#### Test 1: Controllers/CoursesController.cs
Expected findings:
- Duplicated file upload validation (DRY violation)
- `.Single()` usage that can throw (correctness)
- Missing `AsNoTracking()` on read-only queries (performance)

#### Test 2: Controllers/StudentsController.cs
Expected findings:
- `.Single()` vs `.SingleOrDefault()` in Details action
- Broad exception catching
- Search doesn't use `StringComparison` parameter

#### Test 3: Services/NotificationService.cs
Expected findings:
- "Everyone" full control permission on MSMQ queue (security)
- Missing retry logic for MSMQ operations
- Exception handling in Dispose

### 4. Evaluate the Agent

After running the tests:
- Did the agent find the expected issues?
- Did it find issues you didn't expect?
- Were there any false positives?
- Was the severity classification accurate?
- How could the agent definition be improved?

## Deliverable
Write a short summary (3-5 bullet points) of what you learned about effective agent design.
