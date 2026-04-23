# View Generator for Contoso University

## Context
Views use Razor syntax (`.cshtml`) with Bootstrap 5.3.3 styling.
Follow the existing view patterns in `Views/Students/` as the reference.

## Template Variables
- {{EntityName}}: The entity to generate views for
- {{Properties}}: Columns/fields to display
- {{RelatedEntities}}: Dropdown data for foreign key fields

## Requirements

### Index View
- `@model PaginatedList<{{EntityName}}>`
- Bootstrap 5 `<table class="table">` with striped rows
- Column headers as sort links (using `@Html.ActionLink`)
- Search form with text input and "Search" button
- Pagination controls (Previous / Next) using `PaginatedList` properties
- Action links per row: Edit | Details | Delete

### Details View
- `@model {{EntityName}}`
- `<dl>` definition list showing all properties with `@Html.DisplayNameFor` / `@Html.DisplayFor`
- Back to List link

### Create View
- `@model {{EntityName}}`
- `@using (Html.BeginForm()) { @Html.AntiForgeryToken() ... }`
- `@Html.ValidationSummary(true)` at top of form
- `@Html.LabelFor` + `@Html.EditorFor` + `@Html.ValidationMessageFor` per field
- `@Html.DropDownList` for foreign key fields (populated via ViewBag)
- Submit button with Bootstrap `btn btn-primary` class

### Edit View
- Same as Create but with `@Html.HiddenFor(m => m.ID)`
- Pre-populated fields

### Delete View
- `@model {{EntityName}}`
- Display entity details in read-only format
- Confirmation form with `@Html.AntiForgeryToken()`
- "Delete" submit button with `btn btn-danger` class
- "Back to List" link

## Styling
- All views use `@{ ViewBag.Title = "..."; }` for page titles
- Forms use Bootstrap grid: `<div class="form-group">` layout
- Buttons use Bootstrap classes: `btn-primary`, `btn-danger`, `btn-default`
- Tables use `class="table table-striped"`

## Reference
See `Views/Students/*.cshtml` for the canonical examples.
