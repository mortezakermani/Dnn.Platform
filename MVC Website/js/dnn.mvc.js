(function ($) {

    var actions;

    $.fn.dnnPopup = function (options) {

        // Extend our default options with those provided.
        // Note that the first argument to extend is an empty
        // object – this is to keep from overriding our "defaults" object.
        var opts = $.extend({}, $.fn.dnnPopup.defaults, options);

        var actionUrl = opts.actionUrl;
        var data = opts.data;
        var buttons = opts.buttons;

        $.ajax({
            url: actionUrl,
            datatype: "html",
            contenttype: "application/html; charset=utf-8",
            type: "GET",
            data: data,
            success: function (result) {
                var $modal = $(result).filter("#dnnModal");
                $("body").append($modal);
                $modal.modal();

                var $footer = $modal.find(".modal-footer");
                for (var i = 0; i < buttons.length; i++) {
                    var button = buttons[i];
                    var $button = $('<button/>');
                    $button.text(button.text);
                    $button.addClass("btn");
                    $button.click(button.action);

                    if (button.primary) {
                        $button.addClass('btn-primary');
                    } else {
                        $button.addClass('btn-default');
                    };

                    if (button.dismiss) {
                        $button.attr("data-dismiss", "modal");
                    }
                    $footer.append($button);
                }

                $modal.on('hidden.bs.modal', function (e) {
                    $modal.remove();
                });
            },
            error: function(xhr, status) {
            }
        });

        return this;
    };

    // Plugin defaults – added as a property on our plugin function.
    $.fn.dnnPopup.defaults = {
        buttons: [],
        actionUrl: "/",
        data: {}
    };
}(jQuery));