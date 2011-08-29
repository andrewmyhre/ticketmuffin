var geocoder;
var myLatlng;
var myOptions;
var map;
var marker;

$(function () {
    $("#startDate").datepicker({ dateFormat: 'dd/mm/yy' });

    $('#AddressLine').change(lookupAddress);
    $('#City').change(lookupAddress);
    $('#Country').change(lookupAddress);
    $('#Postcode').change(lookupAddress);

    $('#Description').markItUp(markdownSettings);
});

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
