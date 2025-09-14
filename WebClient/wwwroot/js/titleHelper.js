document.addEventListener("DOMContentLoaded", function () {
    titleHelper.getTitle()
    Blazor.addEventListener("enhancedload", function () {
        window.titleHelper.getTitle();
    });
});
window.titleHelper = {
    getTitle: function(){
        document.getElementById("pageTitle").innerText = document.title
    }
}