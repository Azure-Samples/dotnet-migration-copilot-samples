# Exercise: Create Your Own Agent

## Objective
Design and build a custom Copilot agent tailored to the Contoso University codebase.

## Choose Your Agent Type

Pick one of these agent ideas (or create your own):

### Option A: Performance Analyzer Agent
Focus on identifying performance bottlenecks:
- EF Core query patterns (N+1, missing includes, cartesian explosions)
- Memory allocations (string concatenation in loops, LINQ materializations)
- Caching opportunities
- Database index suggestions based on query patterns

### Option B: Accessibility Auditor Agent
Focus on web accessibility in Razor views:
- Missing `alt` attributes on images
- Form labels not associated with inputs
- Color contrast issues in CSS
- Missing ARIA attributes
- Keyboard navigation gaps
- Screen reader compatibility

### Option C: Test Coverage Agent
Focus on identifying untested code paths:
- Controller actions without corresponding tests
- Business logic in controllers that should be in services
- Edge cases not covered (null inputs, empty collections, boundary values)
- Suggest specific test cases for each finding

### Option D: Security Hardening Agent
Focus specifically on security:
- Authentication/authorization gaps
- Input validation completeness
- CSRF protection verification
- File upload security (path traversal, file type spoofing)
- Error handling information leakage
- HTTP security headers

## Agent Definition Template

```markdown
# [Agent Name]

## Role
[Who is this agent? What expertise does it bring?]

## Context
[Project-specific information the agent needs]

## Analysis Categories
[What does the agent look for? Be specific.]

### Category 1: [Name]
- [Specific check 1]
- [Specific check 2]
- ...

### Category 2: [Name]
- ...

## Severity Levels
[Define 3-4 levels with clear criteria]

## Output Format
[Define the report structure]
```

## Steps

1. **Choose your agent type** from the options above
2. **Create the agent file**: `exercises/agent-creation/[your-agent-name].agent.md`
3. **Define the role**: Be specific about the expertise and focus area
4. **Add context**: Include Contoso University-specific details the agent needs
5. **Define categories**: At least 3 analysis categories with specific checks
6. **Set severity levels**: Clear criteria for each level
7. **Design output format**: Structured, actionable, with code examples

## Testing Your Agent

After creating the agent, test it on these files:
1. `Controllers/CoursesController.cs` — has file upload logic (good for security/performance)
2. `Views/Students/Index.cshtml` — has tables and forms (good for accessibility)
3. `Data/SchoolContext.cs` — has EF Core config (good for performance/test coverage)

## Evaluation
- Does the agent find real issues?
- Are the severity levels appropriate?
- Is the output actionable (developer knows exactly what to fix)?
- Would you trust this agent's recommendations?
