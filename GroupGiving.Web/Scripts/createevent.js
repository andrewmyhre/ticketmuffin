var geocoder;
var myLatlng;
var myOptions;
var map;
var marker;

$(document).ready(function () {
    $("#startDate").datepicker({ dateFormat: 'dd/mm/yy' });

    $('#AddressLine').change(lookupAddress);
    $('#City').change(lookupAddress);
    $('#Country').change(lookupAddress);
    $('#Postcode').change(lookupAddress);

    //$('#Description').markItUp(markdownSettings);

    $('#Title').change(function () {
        $('#ShortUrl').val(sanitizeTitleForUrl($('#Title').val()));
        checkUrlAvailability();
    });

    $('#ShortUrl').change(checkUrlAvailability);
    $('#ShortUrl').focusout(checkUrlAvailability);

    //$('#pp-verify-button').click(VerifyPayPalAccount);
    //$('#pp-email').change(VerifyPayPalAccount);
});

function checkUrlAvailability() {
    $('#ShortUrl').val(sanitizeTitleForUrl($('#ShortUrl').val()));

    $.get('/createevent/check-url-availability/?shortUrl=' + $('#ShortUrl').val())
            .success(function (response) {
                if (response == "available")
                    $('#urlAvailability').html('<span class="url-not-available">This url is not available</span>');
                else if (response == "invalid")
                    $('#urlAvailability').html('<span class="url-not-available">This url is not valid</span>');
                else
                    $('#urlAvailability').html('<span class="url-available">Available!</span>');
            })
            .error(function (response) {
                $('#urlAvailability').html('<span class="url-available">Available!</span>');
            });
}

function sanitizeTitleForUrl(text)
{
    var patternLetters = /[öäüÖÄÜáàâéèêúùûóòôÁÀÂÉÈÊÚÙÛÓÒÔß ]/g;

    var lookupLetters = {
        "ä": "a", "ö": "o", "ü": "u",
        "Ä": "A", "Ö": "O", "Ü": "U",
        "á": "a", "à": "a", "â": "a",
        "é": "e", "è": "e", "ê": "e",
        "ú": "u", "ù": "u", "û": "u",
        "ó": "o", "ò": "o", "ô": "o",
        "Á": "A", "À": "A", "Â": "A",
        "É": "E", "È": "E", "Ê": "E",
        "Ú": "U", "Ù": "U", "Û": "U",
        "Ó": "O", "Ò": "O", "Ô": "O",
        "ß": "s", " ": "-"
    };

    var letterTranslator = function (match) {
        return lookupLetters[match] || match;
    }

    text = text.replace(/\//g, " ");
    text = text.replace(patternLetters, letterTranslator);
    text = text.replace(/[^\w\s-_]/g, "-");
    text = text.replace(/\s+/g, " ");
    text = text.replace(/ /gi, "-");
    text = text.replace( /--/gi , "-");
    text = text.toLowerCase();
    return text;
}

// file encoding must be UTF-8!
function getTextExtractor() {
    return (function () {
        var patternLetters = /[öäüÖÄÜáàâéèêúùûóòôÁÀÂÉÈÊÚÙÛÓÒÔß]/g;
        var patternDateDmy = /^(?:\D+)?(\d{1,2})\.(\d{1,2})\.(\d{2,4})$/;
        var lookupLetters = {
            "ä": "a", "ö": "o", "ü": "u",
            "Ä": "A", "Ö": "O", "Ü": "U",
            "á": "a", "à": "a", "â": "a",
            "é": "e", "è": "e", "ê": "e",
            "ú": "u", "ù": "u", "û": "u",
            "ó": "o", "ò": "o", "ô": "o",
            "Á": "A", "À": "A", "Â": "A",
            "É": "E", "È": "E", "Ê": "E",
            "Ú": "U", "Ù": "U", "Û": "U",
            "Ó": "O", "Ò": "O", "Ô": "O",
            "ß": "s"
        };
        var letterTranslator = function (match) {
            return lookupLetters[match] || match;
        }

        return function (node) {
            var text = $.trim($(node).text());
            var date = text.match(patternDateDmy);
            if (date)
                return [date[3], date[2], date[1]].join("-");
            else
                return text.replace(patternLetters, letterTranslator);
        }
    })();
}

function lookupAddress() {
    if ($('#Postcode').val() == '') {
        return;
    }

    if (!map) {
        $('#map').show();
        geocoder = new google.maps.Geocoder();
        myLatlng = new google.maps.LatLng(0, 0);
        myOptions = {
            zoom: 15,
            center: myLatlng,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            mapTypeControl: false,
            disableDoubleClickZoom: true,
            streetViewControl: false
        }
        map = new google.maps.Map($('#map').get(0), myOptions);
        marker = new google.maps.Marker({
            position: myLatlng,
            map: map,
            title: "Drag Me",
            draggable: true
        });
    }

    var addressString = /*$('#Venue').val() + ', ' + */$('#AddressLine').val() + ', ' + $('#City').val() + ' ' + $('#Postcode').val() + ', ' + $('#Country').val();

    geocoder.geocode({ 'address': addressString }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            setPosition(
                results[0].geometry.location,
                results[0].geometry.viewport
            );
            $('#map').show();
        } else {
            $('#map').hide();
        }
    });

    function setPosition(latLng, viewport) {
        var lat = RoundDecimal(latLng.lat(), 6);
        var lng = RoundDecimal(latLng.lng(), 6);
        marker.setPosition(latLng);
        if (viewport) {
            map.fitBounds(viewport);
            map.setZoom(map.getZoom() + 2);
        } else {
            map.panTo(latLng);
        }
        $('#Latitude').val(lat);
        $('#Longitude').val(lng);
    }

    function RoundDecimal(num, decimals) {
        var mag = Math.pow(10, decimals);
        return Math.round(num * mag) / mag;
    };
}
