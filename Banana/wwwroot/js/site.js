var textarea = $(".textarea-content");
function resize() {
    textarea.each(function () {
        $(this).innerHeight(300);
        $(this).innerHeight($(this).prop("scrollHeight"));
    });
}

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

    // for ManageCourse.cshtml
    $("#dialog-add-page").on("dialogopen", function () {
        $("#dialog-section-number-textbox").val("");
        $("#dialog-title-textbox").val("");
    });

    // EditPage.cshtml
    resize();
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

$(".textarea-content").keydown(function () {
    setTimeout(resize, 0);
});

$(".textarea-content").on("drop", function () {
    setTimeout(resize, 0);
});

$(".textarea-content").scroll(function () {
    $(this).scrollTop(0);
});

var $root = $("html, body");
$('a[href^="#"]').click(function () {
    var href = $.attr(this, "href");
    if (href != "#")
        $root.animate({
            scrollTop: $(href).offset().top - 60
        }, 500);
    return false;
});
