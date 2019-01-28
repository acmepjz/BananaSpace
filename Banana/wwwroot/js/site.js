var textarea = $(".textarea-code");
var codemirror = null;

$(function () {
    // initialize dialogs
    $(".dialog").each(function () {
        $(this).dialog({
            appendTo: ".container",
            autoOpen: false,
            closeText: "关闭",
            modal: true,
            resizable: false,
            width: 400
        });
    }); 

    // ManageCourse.cshtml
    $("#dialog-add-page").on("dialogopen", function () {
        $("#dialog-section-number-textbox").val("");
        $("#dialog-title-textbox").val("");
    });

    // EditPage.cshtml
    if (textarea.length == 1) {
        codemirror = CodeMirror.fromTextArea(textarea[0], {
            lineNumbers: true,
            lineWrapping: true,
            viewportMargin: Infinity,
            matchBrackets: true,
            mode: "tex"
        });
        codemirror.init(codemirror)
    }
});

$(".list-item-unsel").click(function () {
    $(".list-item-sel").each(function () {
        $(this).removeClass("list-item-sel")
            .addClass("list-item-unsel");
    });
    $(this).removeClass("list-item-unsel")
        .addClass("list-item-sel");
});

$(".dialog-button").click(function () {
    $(".dialog-open").each(function () {
        $(this).removeClass("dialog-open").dialog("close");
    });
});

$(".link-publish-changes").each(function () {
    $(this).click(function () {
        $("#dialog-publish-changes").addClass("dialog-open").dialog("open");
        $(".dialog-open button.dialog-button-confirm")
            .attr("formaction", "/ManageCourse?handler=Publish&id=" + $(this).attr("data-tag"));
    });
});

$(".link-publish-all").each(function () {
    $(this).click(function () {
        $("#dialog-publish-all").addClass("dialog-open").dialog("open");
        $(".dialog-open button.dialog-button-confirm")
            .attr("formaction", "/ManageCourse?handler=PublishAll&id=" + $(this).attr("data-tag"));
    });
});

$(".link-add-page").each(function () {
    $(this).click(function () {
        $("#dialog-add-page").addClass("dialog-open").dialog("open");
        $(".dialog-open button.dialog-button-confirm")
            .attr("formaction", "/ManageCourse?handler=AddPage&id=" + $(this).attr("data-tag") + "&option=" + $(this).attr("data-tag-option"));
    });
});

$(".link-delete-page").each(function () {
    $(this).click(function () {
        $("#dialog-delete-page").addClass("dialog-open").dialog("open");
        $(".dialog-open button.dialog-button-confirm")
            .attr("formaction", "/ManageCourse?handler=DeletePage&id=" + $(this).attr("data-tag"));
    });
});

$(".leave-no-confirm").click(function () {
    window.onbeforeunload = null;
});

var $root = $("html, body");
$('a[href^="#"]').click(function () {
    var href = $.attr(this, "href");
    if (href != "#")
        $root.animate({
            scrollTop: $(href).offset().top
        }, 500);
    return false;
});
