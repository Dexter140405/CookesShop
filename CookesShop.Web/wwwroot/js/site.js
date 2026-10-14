// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    // Fetch initial cart count on page load
    $.get('/Cart/GetCartCount', function (data) {
        if (data && data.count !== undefined) {
            $('#cart-count').text(data.count);
        }
    });

    // Add to cart from lists (Home, Product Index)
    $('.btn-add-cart').click(function (e) {
        e.preventDefault();
        e.stopPropagation();
        var productId = $(this).data('id');
        addToCart(productId, 1);
    });

    // Add to cart from Details page
    $('#btn-add-cart-detail').click(function (e) {
        e.preventDefault();
        var productId = $(this).data('id');
        var qty = parseInt($('#quantity').val()) || 1;
        addToCart(productId, qty);
    });

    function addToCart(productId, quantity) {
        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { productId: productId, quantity: quantity },
            success: function (res) {
                if (res.success) {
                    $('#cart-count').text(res.totalItems);
                    // Add a small animation to cart icon
                    $('#cart-count').parent().css('transform', 'scale(1.2)');
                    setTimeout(function() {
                        $('#cart-count').parent().css('transform', 'scale(1)');
                    }, 200);
                    
                    // You could show a toast here if you wanted to.
                } else {
                    alert(res.message || 'Có lỗi xảy ra.');
                }
            },
            error: function () {
                alert('Không thể kết nối đến máy chủ.');
            }
        });
    }
});
