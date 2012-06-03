var currencySymbol;
$(document).ready(function () {
    currencySymbol = $('#c').html();
    updateTotals();
    $('select[name=quantity]').change(function () { updateAttendeeList($('select[name=quantity]').val()); updateTotals(); });
});

function updateAttendeeList(quantity) {
    while ($('#attendeeList').children().length > quantity) {
        $('#attendeeList').children().last().remove();
    }

    while ($('#attendeeList').children().length < quantity) {
        var att = $('#attendeeTemplate').clone();
        att.show().appendTo('#attendeeList');
        att.find('.attendeeNumber').html($('#attendeeList').children().length);
        att.find('input[name=attendeeName]').watermark();
    }
    
}

function updateTotals() {
    var total = $('input[name=ticketPrice]').val();
    var quantity = $('select[name=quantity]').val();
    //$('.total').html(currencySymbol + (total * quantity).formatMoney(2, '.', ','));
    $('.total').html(ticketPrices.values[$('select[name=quantity]').prop('selectedIndex')]);
}

Number.prototype.formatMoney = function (c, d, t) {
    var n = this, c = isNaN(c = Math.abs(c)) ? 2 : c, d = d == undefined ? "," : d, t = t == undefined ? "." : t, s = n < 0 ? "-" : "", i = parseInt(n = Math.abs(+n || 0).toFixed(c)) + "", j = (j = i.length) > 3 ? j % 3 : 0;
    return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
};