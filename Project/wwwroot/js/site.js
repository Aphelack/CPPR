// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Ajax pagination - only for product list with #product-list container
$(document).ready(function() {
    // Only apply Ajax pagination if #product-list exists (not on admin pages)
    if ($('#product-list').length > 0) {
        $(document).on('click', '.page-link', function(e) {
            e.preventDefault();
            var url = $(this).attr('href');
            
            if (url && url !== '#') {
                $.ajax({
                    url: url,
                    type: 'GET',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    success: function(data) {
                        $('#product-list').html(data);
                    },
                    error: function() {
                        console.error('Error loading page');
                    }
                });
            }
        });
    }
});
