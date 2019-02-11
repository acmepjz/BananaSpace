var fileId = 0, onConfirm = false;

$(function () {
    // initialize dialogs
    $('.dialog').each(function () {
        $(this).dialog({
            appendTo: '.container',
            autoOpen: false,
            closeText: '关闭',
            modal: true,
            resizable: false,
            width: 400
        });
    }); 

    // ViewPage.cshtml
    $('#_btn-fav').click(function () {
        var $this = $(this);
        if ($this.attr('disabled')) return;
        $this.attr('disabled', 'disabled');

        var pageId = $this.attr('data-page-id'),
            data = new FormData(),
            action = $this.attr('data-action'),
            rvt = $('input[name="__RequestVerificationToken"]').val();
        data.append('Action', action);

        $.ajax({
            url: '/page/' + pageId,
            data: data,
            processData: false,
            contentType: false,
            headers: { "RequestVerificationToken": rvt },
            type: 'POST',
            success: function () {
                $this.removeAttr('disabled').attr('data-action', action == 'add-fav' ? 'cancel-fav' : 'add-fav')
                .text(action == 'add-fav' ? '取消收藏' : '收藏页面');
            }
        });
    });

    // ManageCourse.cshtml
    $('#dialog-add-page').on('dialogopen', function () {
        $('#dialog-section-number-textbox').val('');
        $('#dialog-title-textbox').val('');
    });

    // CodeMirror
    var $textarea = $('.textarea-code'), codemirror;
    if ($textarea.length > 0 && CodeMirror) {
        codemirror = CodeMirror.fromTextArea($textarea[0], {
            lineNumbers: true,
            lineWrapping: true,
            viewportMargin: Infinity,
            mode: 'tex',
            readOnly: $textarea.attr('readonly') ? true : false
        });
        codemirror.init(codemirror)
    }

    // syntax highlighting
    if (typeof CodeMirror !== 'undefined' && CodeMirror.colorize)
        CodeMirror.colorize($('.code-snippet'), 'tex');

    // EditPage.cshtml
    if ($('.editor-container').length > 0) {
        $('.editor-tab-link').click(function () {
            if ($(this).parents('.editor-tab-link-inactive').length === 0) return;
            // there are only 2 tabs, so we simply need to swap them
            var $active = $('.editor-tab-active').removeClass('editor-tab-active'),
                $inactive = $('.editor-tab-inactive').removeClass('editor-tab-inactive'),
                $linkActive = $('.editor-tab-link-active').removeClass('editor-tab-link-active'),
                $linkInactive = $('.editor-tab-link-inactive').removeClass('editor-tab-link-inactive');
            $active.addClass('editor-tab-inactive');
            $inactive.addClass('editor-tab-active');
            $linkActive.addClass('editor-tab-link-inactive');
            $linkInactive.addClass('editor-tab-link-active');
        });

        // override Ctrl+S
        $(document).keydown(function (e) {
            var key = undefined;
            var possible = [e.key, e.keyIdentifier, e.keyCode, e.which];
            while (key === undefined && possible.length > 0)
                key = possible.pop();

            if (key && (key == 115 || key == 83) && (e.ctrlKey || e.metaKey) && !(e.altKey)) {
                e.preventDefault();
                $('.editor-save-button').trigger('click');
                return false;
            }
            return true;
        }); 

        var $form = $('.editor-container form'),
            pageId = $form.attr('data-page-id'),
            rvt = $('input[name="__RequestVerificationToken"]').val();
        $('.editor-save-button').click(function () {
            var $this = $(this);
            if ($this.attr('disabled')) return;

            // TODO: VALIDATE

            $this.attr('disabled', 'disabled').text('正在保存...');

            prepareForSaving();

            var data = new FormData($form[0]);
            data.append('Action', 'save');
            $.ajax({
                url: '/page/' + pageId + '/edit',
                data: data,
                processData: false,
                contentType: false,
                headers: { "RequestVerificationToken": rvt },
                type: 'POST',
                complete: function () {
                    $this.removeAttr('disabled').text('保存并预览');
                },
                success: function (data) {
                    $('.previewer-container .box').html(data);
                    if (MathJax)
                        MathJax.Hub.Queue(['Typeset', MathJax.Hub]);
                    if (CodeMirror.colorize)
                        CodeMirror.colorize($('.code-snippet'), 'tex');
                    window.onbeforeunload = null;
                },
                error: function () {
                    // .........
                    alert('保存失败。');
                }
            });
        })
        $('.editor-publish-button').click(function () {
            prepareForSaving();
            $form.append($('<input name="Action" type="hidden" value="publish">')).submit();
        })

        function prepareForSaving() {
            if (codemirror !== undefined)
                $textarea.text(codemirror.getValue());
        }
    }

    // file uploading
    var fileInput = $('#_file-input');
    if (fileInput.length > 0) {
        fileId = 1000;
        var rvt = $('input[name="__RequestVerificationToken"]').val(),
            pageId = fileInput.attr('data-page-id');

        $('.file-upload-link').click(function () {
            $('#_file-input').trigger('click');
        });

        $('.file-delete-link').click(onDelete);

        fileInput.change(function () {
            if (fileInput.val() == '') return;

            var thisFileId = fileId++,
                file = fileInput[0].files[0],
                errorMsg = false, confirmMsg = false;
            fileInput.val('');

            if (!/\.(jpg|jpeg|png|svg)$/i.test(file.name))
                errorMsg = '只能上传 JPEG, PNG 或 SVG 图片。';
            else if (file.size >= 2 << 20) // 2 MB
                errorMsg = '文件大小不能超过 2 MB。';
            else {
                var toReplace = false;
                $('.file-list-item').each(function () {
                    var text = $(this).find('.file-list-item-name').text();
                    if (text.localeCompare(file.name, 'en', { sensitivity: 'base' }) == 0)
                        toReplace = $(this).attr('data-id');
                });
                if (toReplace) confirmMsg = '上传此文件会替换原有的文件。确认上传吗？';
            }

            if (errorMsg || confirmMsg) {
                var dialog = $('.dialog-confirm');
                dialog.find('.dialog-content').text(errorMsg || confirmMsg);
                onConfirm = confirmMsg ? function () { doUpload(thisFileId, file, toReplace); } : false;
                $('.ui-dialog-title').text('上传文件');
                dialog.addClass('dialog-open').dialog('open');
                return;
            }

            doUpload(thisFileId, file);
        });

        function onDelete() {
            var thisFileId = $(this).parents('.file-list-item').attr('data-id');

            var dialog = $('.dialog-confirm');
            dialog.find('.dialog-content').text('确认删除此文件吗？删除后将无法恢复，只能重新上传。');
            onConfirm = function () { doDelete(thisFileId); };
            $('.ui-dialog-title').text('确认删除');
            dialog.addClass('dialog-open').dialog('open');
        }

        function doUpload(thisFileId, file, toReplace) {
            if (toReplace !== undefined)
                $('.file-list-item[data-id=' + toReplace + ']').remove();

            $('<div>', {
                'class': 'file-list-item list-item',
                'data-id': thisFileId,
                'html': $('<div>', {
                    'class': 'file-list-item-name',
                    'title': file.name,
                }).text(file.name).add($('<div>', {
                    'class': 'file-progress-container',
                    'html': $('<div>', {
                        'class': 'file-progress',
                        'html': $('<div>', {
                            'class': 'file-progress-inner',
                        })
                    })
                })).add($('<div>', {
                    'html': $('<a>', {
                        'class': 'file-delete-link link-no-underline',
                        'href': '#'
                    }).text('删除').click(onDelete).css('display', 'none')
                }))
            }).appendTo('.file-list');

            var data = new FormData();
            data.append('PageId', pageId);
            data.append('Action', 'upload');
            data.append('File', file);

            $.ajax({
                url: '/UploadFile',
                data: data,
                headers: { "RequestVerificationToken": rvt },
                processData: false,
                contentType: false,
                type: 'POST',
                xhr: function () {
                    var xhr = new window.XMLHttpRequest();
                    xhr.upload.addEventListener('progress', function (evt) {
                        if (evt.lengthComputable) {
                            var percent = evt.loaded / evt.total * 100;
                            $('.file-list-item[data-id=' + thisFileId + '] .file-progress-inner').css('width', percent + '%');
                        }
                    }, false);
                    return xhr;
                },
                success: function () {
                    $('.file-list-item[data-id=' + thisFileId + '] .file-progress-container').html('上传成功');
                    $('.file-list-item[data-id=' + thisFileId + '] .file-delete-link').css('display', '');
                },
                error: function () {
                    $('.file-list-item[data-id=' + thisFileId + '] .file-progress-container').html('上传失败');
                }
            });
        }

        function doDelete(thisFileId) {
            var fileName = $('.file-list-item[data-id=' + thisFileId + '] .file-list-item-name').text();

            var data = new FormData();
            data.append('PageId', pageId);
            data.append('Action', 'delete');
            data.append('FileName', fileName);

            $('.file-list-item[data-id=' + thisFileId + '] .file-progress-container').html('正在删除');
            $('.file-list-item[data-id=' + thisFileId + '] .file-delete-link').css('display', 'none');

            $.ajax({
                url: '/UploadFile',
                data: data,
                headers: { "RequestVerificationToken": rvt },
                processData: false,
                contentType: false,
                type: 'POST',
                success: function () {
                    $('.file-list-item[data-id=' + thisFileId + ']').remove();
                },
                failed: function () {
                    $('.file-list-item[data-id=' + thisFileId + '] .file-progress-container').html('删除失败');
                }
            });
        }
    }
});

$('.list-item-unsel').click(function () {
    $('.list-item-sel').each(function () {
        $(this).removeClass('list-item-sel')
            .addClass('list-item-unsel');
    });
    $(this).removeClass('list-item-unsel')
        .addClass('list-item-sel');
});

$('.dialog-button').click(function () {
    $('.dialog-open').each(function () {
        $(this).removeClass('dialog-open').dialog('close');
    });
});

$('.dialog-button-confirm').click(function () {
    if (onConfirm) onConfirm();
    onConfirm = false;
});

$('.link-confirm').click(function () {
    $('.dialog-confirm').addClass('dialog-open').dialog('open');
    var $this = $(this), $dialog = $('.dialog-confirm').parents('.ui-dialog');
    $dialog.find('input[name=Action]').val($this.attr('data-action'));
    $dialog.find('input[name=PageId]').val($this.attr('data-page-id'));
    $dialog.find('.ui-dialog-title').text($this.attr('data-title'));
    $dialog.find('.dialog-content').text($this.attr('data-message'));
});

$('.link-add-page').click(function () {
    $('.dialog-add-page').addClass('dialog-open').dialog('open');
    var $this = $(this), $dialog = $('.dialog-add-page').parents('.ui-dialog');
    $dialog.find('input[name=Action]').val($this.attr('data-action'));
    $dialog.find('input[name=PageId]').val($this.attr('data-page-id'));
});

$('.link-delete').click(function () {
    $('.dialog-delete').addClass('dialog-open').dialog('open');
    var $this = $(this), $dialog = $('.dialog-delete').parents('.ui-dialog');
    $dialog.find('input[name=Action]').val($this.attr('data-action'));
    $dialog.find('input[name=PageId]').val($this.attr('data-page-id'));
    $dialog.find('.ui-dialog-title').text($this.attr('data-title'));
    $dialog.find('.dialog-content').text($this.attr('data-message'));
});

$('.leave-no-confirm').click(function () {
    window.onbeforeunload = null;
});

var $root = $('html, body');
$('a[href^="#"]').click(function () {
    var href = $.attr(this, 'href');
    if (href != '#')
        $root.animate({
            scrollTop: $(href).offset().top
        }, 500);
    return false;
});
