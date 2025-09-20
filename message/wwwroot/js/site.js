document.getElementById("mapBtn").addEventListener("click", function () {
    document.getElementById("mapModal").style.display = "block";
    initMap();
});

function closeMap() {
    document.getElementById("mapModal").style.display = "none";
}

function initMap() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            const userLocation = {
                lat: position.coords.latitude,
                lng: position.coords.longitude
            };

            const map = new google.maps.Map(document.getElementById("map"), {
                center: userLocation,
                zoom: 4 // zoomed out to see the world
            });

            new google.maps.Marker({
                position: userLocation,
                map: map,
                title: "You are here!"
            });
        }, function () {
            alert("Unable to fetch your location.");
        });
    } else {
        alert("Geolocation not supported.");
    }
}