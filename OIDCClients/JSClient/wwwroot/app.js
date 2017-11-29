function log() {
    document.getElementById('results').innerText = '';

    Array.prototype.forEach.call(arguments, function (msg) {
        if (msg instanceof Error) {
            msg = "Error: " + msg.message;
        }
        else if (typeof msg !== 'string') {
            msg = JSON.stringify(msg, null, 2);
        }
        document.getElementById('results').innerHTML += msg + '\r\n';
    });
}

document.getElementById("login").addEventListener("click", login, false);
document.getElementById("api").addEventListener("click", api, false);
document.getElementById("logout").addEventListener("click", logout, false);

var config = {
    authority: "http://rum-zakharov.scanex.ru:5000",
    client_id: "js",
    redirect_uri: "http://rum-zakharov.scanex.ru:5003/callback.html",
    silent_redirect_uri: "http://rum-zakharov.scanex.ru:5003/callback-silent.html",
    automaticSilentRenew: true,
    silentRequestTimeout:10000,
    response_type: "id_token token",
    scope:"openid profile email py_api",
    post_logout_redirect_uri : "http://rum-zakharov.scanex.ru:5003/test.html",
};
//Oidc.Log.logger = window.console;
var mgr = new Oidc.UserManager(config);
mgr.events.addUserLoaded(function (user) {
    log("User logged in", user)
});
mgr.signinSilent()
.then((r)=>{console.log(r)})
.catch((err)=>{
    if (err.error=='login_required')
        log("User not logged in");
    else if(err.error=='consent_required')
        log("User does not grant access");
    console.log(err)
})
//mgr.getUser()
// .then(function (user) {
//     if (user) {
//         log("User logged in", user.profile);
//     }
//     else {
//         log("User not logged in");
//     }
// });

function login() {
    mgr.signinRedirect();
}

function api() {
    mgr.getUser().then(function (user) {
        var url = "http://localhost:5001/api/1.0/themes2";

        // fetch(url)
        // .then(function(response) {
        //   return response;
        //  })
        //  .then(function(response) {
        //    return response;
        //   })
        // .then(function(user) {
        //   console.log(user); // iliakan
        // })
        // .catch( alert );
   
        var xhr = new XMLHttpRequest();
        xhr.open("GET", url);
        xhr.onload = function () {
            log(xhr.status, JSON.parse(xhr.responseText));
        }
        xhr.setRequestHeader("Authorization", "Bearer " + user.access_token);
        xhr.send();
    });
}

function logout() {
    mgr.signoutRedirect();
}
