# Lab 2: Razor Language, Partial Views, View Components

## Theory
In this lab, we explored the Razor syntax and how to modularize views using Partial Views and View Components.

### Key Concepts
- **Razor Syntax**: Mixing C# with HTML (e.g., `@foreach`, `@if`).
- **Tag Helpers**: Server-side attributes that transform into HTML (e.g., `asp-controller`, `asp-action`).
- **Partial Views**: Reusable chunks of Razor markup (`_MenuPartial`, `_UserInfoPartial`).
- **View Components**: More powerful than partials, can have logic in a C# class (`CartViewComponent`).
- **ViewData/ViewBag**: Passing data from Controller to View.

## Implementation Details
- **Tag Helpers**: Updated `_Layout.cshtml` to use `asp-*` helpers for navigation.
- **Data Passing**:
    - Passed "Lab 2" title via `ViewData`.
    - Passed a list of `ListDemo` objects as the model to `Index` view.
    - Used `SelectList` and `asp-items` to render a dropdown.
- **Partial Views**:
    - Extracted navigation menu to `_MenuPartial.cshtml`.
    - Extracted user info to `_UserInfoPartial.cshtml`.
    - Implemented "active" class logic for menu items.
- **View Components**:
    - Created `CartViewComponent` to render the cart summary.
    - Invoked it within `_UserInfoPartial`.

## Tools Used
- VS Code
- .NET CLI
- Razor
