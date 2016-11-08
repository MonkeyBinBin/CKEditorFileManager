CKEDITOR.editorConfig = function (config) {
    // Define changes to default configuration here. For example:
    // config.language = 'fr';
    // config.skin = 'kama';
    config.uiColor = '#99CCFF';
    config.extraPlugins = "html5video";

    //video plugin需開啟此設定切換至原始碼模式才不會把內容清除
    config.allowedContent = true;
};
