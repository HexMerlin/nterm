# Select control requirement

This is a control in the context of a CLI. The user can select an item by pressing the enter key. The selected item is returned to the caller. It is similar to a dropdown list in a web context, but it is very limited in the initial version.

## Items

It shows a list of specified items. Each item has the following properties:

* Text: The text that is visible in the list
* A function (callback) that is triggered when the item is selected. The control does not know what the function does. It is up to the caller to define the behavior. The control does not call it either, it returns the selected item and it is up to the caller to call the function.

## Features

* Down arrow key selects the next item, if any.
* Up arrow key selects the previous item, if any.
* Enter key selects the current item.
* Escape key cancels the selection.
* Highlight the selected item in yellow color.
* The selected item should be in the position where the cursor is. That means that previous items are not visible (above).
* The first item is selected by default.
* It is not possible to type while the control is active.

## The control does NOT

* Present the list visually, it only displays the selected item on the line where the cursor is.
* Handle empty lists. If the list is empty, return a "empty" item, with empty string and a callback that does nothing.

## Desired interface

Calling the select controller should look something like this:

```
var items = [
    { Text: "Item 1", Action: () => { Console.WriteLine("Item 1 selected"); } },
    { Text: "Item 2", Action: () => { Console.WriteLine("Item 2 selected"); } },
    { Text: "Item 3", Action: () => { Console.WriteLine("Item 3 selected"); } }
];

var selectedItem = Select.Show(items); // Returns the selected item

```



