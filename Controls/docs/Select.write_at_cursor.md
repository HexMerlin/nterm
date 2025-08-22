# Write at cursor

The current implementation of the select control does not write the selected item at the cursor position. It writes the selected item at the beginning of the line. This is not a good user experience. It should write the selected item at the cursor position and when selecting a new item, it should overwrite the previous item.