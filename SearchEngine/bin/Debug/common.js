function getsuuid(){
	var f = new Date().getUTCMilliseconds();
    return (Math.round(Math.random() * 2147483647) * f) % 10000000000;
}

function getdtsuuid(){
	var e = (new Date).getUTCMilliseconds();
    return Math.round(2147483647 * Math.random()) * e % 1e10;
}