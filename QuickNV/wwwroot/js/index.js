setInterval(async function () {
    await fetch('api/login/heartbeat');
}, 1 * 60 * 1000);