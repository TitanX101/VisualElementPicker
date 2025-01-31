# Visual Element Picker

## Overview
The `VisualElementPicker` attribute simplifies the assignment of visual element references for your `UI script managers`. Using this attribute, you can easily select visual elements associated with the UI document by specifying them as strings within the inspector.

## Installation

### Method 1: Manual Installation

1. **Clone or download** this repository.
2. **Open** your Unity project.
3. **Drag and drop** the package into your `Assets` folder or `Packages` folder.

### Method 2: Unity Package Manager

1. **Open** the Unity Package Manager: `Window -> Package Manager`.
2. **Click** on the `+` button in the top left corner.
3. **Select** `Add package from git URL...`.
4. **Paste** the following URL: [`https://github.com/TitanX101/VisualElementPicker.git`](https://github.com/TitanX101/VisualElementPicker.git)
5. **Click** `Add` to install the package directly from the repository.

## Features
- Apply the `VisualElementPicker` attribute for `string` types and specify if you want to skip null elements (i.e., visual elements without names). **This attribute requires a `UIDocument` component to be present on the same GameObject where the script with this attribute is attached**.
- From the picker, you can apply search filters either by name or by type directly. To search by type, use the same format as in Unity, for example, "t: MyVisualElementType".

![demo1](https://github.com/user-attachments/assets/8ed75473-c920-422b-90f7-146bc46f40ce)
![demo2](https://github.com/user-attachments/assets/3514fbd6-7ebd-4cda-994b-8d40b428589b)

## Usage Examples
Here are some examples of how to use the `VisualElementPicker` attribute:
```csharp
[VisualElementPicker]
public string myElement;

[VisualElementPicker(TargetType = typeof(MyVisualElementType))]
public string myTypedElement;

[VisualElementPicker(targetType: typeof(Button), skipEmpty: true)]
public string myFilteredElement;

[VisualElementPicker(true)]
public string myElementSkippingEmpty;
```
## TODO
Although I'm not sure if this is possible yet, it would be interesting to be able to assign visual element references and ensure they don't get lost once their names are changed from the UI Builder. So this will be a **POSSIBLE** task to do.

