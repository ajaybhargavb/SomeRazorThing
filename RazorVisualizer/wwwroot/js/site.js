// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

( function($) {

    var defaultContent = `
@{
    var foo = "Hello World!";
}

<div>@foo</div>
    `;
    $(document).ready(function() {
        $('textarea#source').val(defaultContent);
    });

    $('textarea#source').bind('input propertychange', function() {
        $.ajax({
            url: 'api/Parse',
            dataType: 'text',
            type: 'post',
            cache: false,
            contentType: 'application/json',
            data: JSON.stringify({ "source": $('#source').val() }),
            success: function(html){
                $("#results").html(html);
            },
            error: function(jqXhr, textStatus, errorThrown){
                console.log("error: " + errorThrown);
            }
        });
    });

    
      



} ) ( jQuery );
