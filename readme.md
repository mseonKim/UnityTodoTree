# Unity Todo Tree
The TODO Tree for Unity Editor
- recommend version: >=2019.4(LTS)

## How To Use
Open TODO Tree editor located in "Window -> TODO Tree" at the top bar.


## Support Features

  1. Tag (TODO, FIXME)
  2. Priority (Minor, Medium, Major)
  3. Progress (Active, On-Hold, Completed)
  4. Reorder (Drag & Drop the todo)
  5. Quick Create Buttons on Inspector

You can edit settings in the TODO Tree window or
manually add more custom items (tag, priority, progress) to the _TodoConfig_ asset. (+ color for each of them)

If you don't see the quick inspector buttons, please reopen Unity editor after you open the TODO Tree window.


## TodoConfig
![image](https://user-images.githubusercontent.com/77778881/107454666-d18be100-6b90-11eb-96a1-8d7e73fe515d.png)

Located in _"Assets/Editor/TodoTree"_ directory.
You can modify the default names and colors in tag, priority, progress.
If you don't want to see the quick create buttons on inspector, just uncheck _"Inspector GUI"_.

**Please Note:**
* The _Tags_ should contain at least 2 items.
* The _Priorities_ and _Progresses_ should contain at least 1 item.
* If you change any name in the _TodoConfig_ asset, close the TODO Tree tab and reopen it to let it sync them.
* The quick inspector buttons only support the default 2 tags (TODO, FIXME). Of course, the names would be different if you modified them already.


## Example

![image](https://user-images.githubusercontent.com/77778881/105460595-5d67c700-5ccf-11eb-8c8b-217fc66744bd.png)
