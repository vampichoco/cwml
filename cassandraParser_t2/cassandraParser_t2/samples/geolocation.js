
var x = document.getElementById("demo");
function getLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(showPosition);
    } else {
        x.innerHTML = "Geolocation is not supported by this browser.";
    }
}
function showPosition(position) {
    var src = "http://maps.google.com/maps/api/staticmap?center=" + position.coords.latitude + "," + position.coords.longitude +"&zoom=15&markers=" + position.coords.latitude + "," + position.coords.longitude + "&size=500x300&sensor=TRUE_OR_FALSE"
    document.getElementById("map").src = src;

}