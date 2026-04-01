using System.Windows;

// Khai báo thông tin về Resource (style, theme) của ứng dụng WPF
[assembly: ThemeInfo(

    // Vị trí chứa ResourceDictionary theo từng theme (Light, Dark...)
    // None = không dùng theme riêng
    // Nếu không tìm thấy resource ở page hoặc app thì cũng không tìm theo theme
    ResourceDictionaryLocation.None,

    // Vị trí chứa ResourceDictionary chung (generic)
    // SourceAssembly = nằm trong chính project hiện tại
    // Nếu không tìm thấy resource ở page, app hoặc theme thì sẽ tìm ở đây
    ResourceDictionaryLocation.SourceAssembly

)]