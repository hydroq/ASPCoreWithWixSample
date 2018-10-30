;
var scenarios = {};

var startX, startY, startWidth, startHeight;
var resizable;

var loginForm;
var activeSc;

var authTicket = '';

var xhr;

var dom = {
  content: {
    title: undefined,
    header: undefined,
    description: undefined,
    link: undefined,
    container: undefined,
    headCont: undefined
  },
  aside: undefined,
  list: {
    categories: undefined,
    products: undefined,
    scenarios: undefined,
    dropHeaders: undefined
  },
  search: undefined,
  resizer: undefined,
  resizable: undefined,
  logout: undefined,
  username: undefined,
  login: {
    form: undefined,
    popup: {
      self: undefined,
      head: undefined,
      usernameField: undefined,
      passwordField: undefined
    },
    container: undefined
  },
  getAllDOM: function (doc) {
    this.aside = doc.querySelector('.aside-container');
    this.username = doc.querySelector('#username');

    this.content.header = doc.querySelector('header');
    this.content.container = doc.querySelector('.content-container');
    this.content.headCont = doc.querySelector('#resizable .head-container');

    this.login.form = doc.querySelector('#login-popup form');
    this.login.container = doc.querySelector('#login-container');

    this.login.popup.self = doc.querySelector('#login-popup');
    this.login.popup.head = doc.querySelector('.popup-head');
    this.login.popup.usernameField = doc.querySelector('#login-popup input[name="user"]').value;
    this.login.popup.passwordField = doc.querySelector('#login-popup input[name="password"]').value;

    this.list.scenarios = doc.querySelectorAll('.scenario');
    this.list.dropHeaders = doc.querySelectorAll('.drop-header');

    this.content.title = doc.querySelector('#resizable .heading');
    this.content.link = document.querySelector('#resizable .start');
    this.content.draft = document.querySelector('#resizable .draft');
    this.content.description = document.querySelector('#resizable .description');

    this.search = doc.querySelector('#search');
  }
};

window.onload = domready;

function domready() {
  dom.getAllDOM(document);
  const authData = getAuthData();
  console.log('authData on load:', authData);

  showLoginForm(false);
  showUserData(false);
  showContent(false);

  if (authData) {
    authTicket = authData.token;
    dom.username.innerHTML = authData.username;
    init();
  } else showLoginForm(true);

  dom.login.form.addEventListener('submit', (ev) => loginSubmit(ev), false);
}

function getAuthData() {
  return localStorage.getItem('auth_data') !== null ? JSON.parse(localStorage.getItem('auth_data')) : null;
}


function init() {
  console.log('init...');
  // get scenarios
  console.log('ticket', authTicket);
  request('GET', 'scenarios?ticket=' + authTicket, true, (res) => {
    console.log('res:', res);
    if (res) {
        try {
        showLoginForm(false);
        scenarios = JSON.parse(res);
        processData();
      } catch (err) {
        console.error(err);
      }
    }
  }, (err) => {
    console.error('scen err:', err);
    if (err.indexOf('401') > -1 || err.indexOf('302') > -1 ) {
      showLoginForm(true);
    }
  });

  listenForLogOut();
}

function listenForLogOut() {
  var logoutEl = document.querySelector('#logout');
  if (logoutEl) {
    logoutEl.addEventListener('click', (event) => {
      console.log(event);
      if (localStorage.getItem('auth_data') !== null)
        localStorage.removeItem('auth_data');
      document.location.reload();
    }, false);
  }
}

function request(type, url, async, callback, errorCallback) {
  var xhr = new XMLHttpRequest();
  xhr.open(type, url, async);
  xhr.onload = function (e) {
    if (xhr.readyState === 4) {
      if (xhr.status === 200) {
        callback(xhr.responseText);
      } else {
        errorCallback(xhr.status + ': ' + xhr.statusText);
      }
    }
  };
  xhr.onerror = () => { errorCallback(xhr.status + ': ' + xhr.statusText); }
  xhr.send();
}

function showContent(status) {
  dom.content.header.style.display = status ? 'flex' : 'none';
  dom.content.container.style.display = status ? 'flex' : 'none';
}

function showLoginForm(status) {
  dom.login.popup.self.style.display = status ? 'flex' : 'none';
  showContent(!status);
  showUserData(!status);
}

function showUserData(status) {
  dom.login.container.style.display = status ? 'block' : 'none';
}

function loginSubmit(ev) {
  ev.preventDefault();
  try { ev.stopPropagation(); } catch (err) { console.error(err); }

  var user = (document.querySelector('#login')).value;
  var password = (document.querySelector('#password')).value;

  request('POST', `login?user=${user}&password=${password}`, true, (res) => {
    console.log('login res:', res);
    if (res) {
      localStorage.setItem('auth_data', JSON.stringify({
        username: user,
        token: res
      }));
      authTicket = getAuthData().token;
      showLoginForm(false);
      dom.username.innerHTML = user;
      init();
    }
  }, (err) => {
    console.error('login error:', err);
    var popupHead = document.querySelector('#login-popup .popup-head')
    popupHead.classList.add('error');
    setTimeout(() => {
      popupHead.classList.remove('error');
    }, 2000);
  });
}

function parseScenarios() {
  for (var key in scenarios) {
    var category = document.createElement('div');
    category.classList.add('category', 'drop-container');
    var label = document.createElement('span');
    label.classList.add('label', 'drop-header');
    label.innerHTML = key;
    label.title = key;
    category.appendChild(label);
    for (var key2 in scenarios[key]) {
      var product = document.createElement('div');
      product.classList.add('product', 'drop-container', 'hidden');
      var label = document.createElement('span');
      label.classList.add('label', 'drop-header');
      label.innerHTML = key2;
      label.title = key2;
      product.appendChild(label);
      category.appendChild(product);
      for (var key3 in scenarios[key][key2]) {
        var scenario = document.createElement('div');
        scenario.classList.add('scenario', 'drop-container', 'hidden');
        scenario.setAttribute('data-name', scenarios[key][key2][key3].name);

        var startUri = scenarios[key][key2][key3].uri;
        var draftUri = scenarios[key][key2][key3].draft;

        console.log('start:', startUri, 'draft:', draftUri);
        var sURI, sDraft;

        if (startUri) {
          sURI = decodeURIComponent(startUri);
          scenario.setAttribute('data-uri', sURI);
        }
        if (draftUri) {
          sDraft = decodeURIComponent(draftUri);
          scenario.setAttribute('data-draft', sDraft);
        }

        scenario.setAttribute('data-id', scenarios[key][key2][key3]['id']);
        scenario.innerHTML = scenarios[key][key2][key3].name;
        scenario.title = scenarios[key][key2][key3].name;
        product.appendChild(scenario);
      }
    }
    dom.aside.appendChild(category);
  }
}

function toggleDropDown(ev) {
  var parent = ev.target.parentElement;
  if (parent && parent.classList.contains('drop-container')) {
    for (var child in parent.childNodes) {
      if (parent.childNodes[child].nodeType == 1 &&
        parent.childNodes[child].classList.contains('drop-container')) {
        if (parent.childNodes[child].classList.contains('hidden')) {
          parent.childNodes[child].classList.remove('hidden');
          parent.classList.add('dropped');
        } else {
          parent.childNodes[child].classList.add('hidden');
          parent.classList.remove('dropped');
        }
      }
    }
  }
}

function findEl(inputVal) {
  for (var i = 0; i < dom.list.scenarios.length; i++) {
    var scns = dom.list.scenarios[i];
    if (!scns.classList.contains('hidden')) scns.classList.add('hidden');
    if (!scns.parentElement.classList.contains('hidden')) {
      scns.parentElement.classList.add('hidden');
      scns.parentElement.classList.remove('dropped');
      scns.parentElement.parentElement.classList.remove('dropped');
    }

    if (((scns.getAttribute('data-name')).toLowerCase()).indexOf(inputVal) > -1) {
      scns.classList.remove('hidden');
      scns.parentElement.classList.remove('hidden');
      scns.parentElement.parentElement.classList.remove('hidden');
      scns.parentElement.classList.add('dropped');
      scns.parentElement.parentElement.classList.add('dropped');
    }
  }
}

function hideContent() {
  dom.content.headCont.style.opacity = 0;
}

function setContent(scenario) {
  console.log('setContent:', scenario);
  dom.content.headCont.style.opacity = '1';
  dom.content.title.innerHTML = scenario.name;
  dom.content.title.title = scenario.name;
  dom.content.link.href = scenario.uri;
  dom.content.draft.href = scenario.draft;
}

function listenforDrop() {
  var drops = document.querySelectorAll('.drop-header');
  for (var i = 0; i < drops.length; i++) {
    var drop = drops[i];
    drop.addEventListener('click', toggleDropDown, false);
  }
}

function initDrag(e) {
  startX = e.changedTouches ? e.changedTouches[0].clientX : e.clientX;

  startWidth = resizable.clientWidth;
  document.documentElement.addEventListener('touchmove', doDrag, false);
  document.documentElement.addEventListener('touchend', stopDrag, false);

  document.documentElement.addEventListener('mousemove', doDrag, false);
  document.documentElement.addEventListener('mouseup', stopDrag, false);
}

function doDrag(e) {
  var curPos = e.changedTouches ? e.changedTouches[0].clientX : e.clientX;
  document.querySelector('#resizable').classList.add('remove-select');
  resizable.style.minWidth = (startWidth + curPos - startX) + 'px';
}

function stopDrag(e) {
  document.querySelector('#resizable').classList.remove('remove-select');
  document.documentElement.removeEventListener('touchmove', doDrag, false);
  document.documentElement.removeEventListener('touchend', stopDrag, false);

  document.documentElement.removeEventListener('mousemove', doDrag, false);
  document.documentElement.removeEventListener('mouseup', stopDrag, false);
}

function toggleUriButtons(el, uri) {
  console.log(el, uri);
  console.log('typeof uri:', typeof uri);
  if (uri) el.classList.remove('hidden');
}

function processData() {
  parseScenarios();
  listenforDrop();
  resizable = document.querySelector('#resizer');
  var resizeTarget = document.querySelector('#resizable');
  resizable.className = resizable.className + ' resizable';
  var resizer = document.createElement('div');
  resizer.className = 'resizer';
  resizeTarget.appendChild(resizer);
  resizer.addEventListener('touchstart', initDrag, false);
  resizer.addEventListener('mousedown', initDrag, false);

  dom.search.addEventListener('input', function (ev) {
    var inpVal = ev.target.value.trim().toLowerCase();
    if (inpVal != '') findEl(inpVal);
  });

  var loader = document.querySelector('#loader');
  var scenariosEl = document.querySelectorAll('.scenario');
  for (var i = 0; i < scenariosEl.length; i++) {
      // if (scenariosEl[i].getAttribute('data-uri')) {
    scenariosEl[i].addEventListener('click', function (ev) {
      if (activeSc) activeSc.classList.remove('active');
      activeSc = ev.target;
      activeSc.classList.add('active');
      var sId = activeSc.getAttribute('data-id');
      console.log('activeSc:', activeSc);
      let uriBtns = document.querySelectorAll('.uri-btn');
      for (var i = 0; i < uriBtns.length; i++) {
          uriBtns[i].classList.add('hidden');
      }
      toggleUriButtons(document.getElementById('start-btn'), activeSc.getAttribute('data-uri'));
      toggleUriButtons(document.getElementById('draft-btn'), activeSc.getAttribute('data-draft'));

      setContent({
          name: activeSc.getAttribute('data-name'),
          uri: activeSc.getAttribute('data-uri'),
          draft: activeSc.getAttribute('data-draft')
      });
      loader.style.display = 'block';
      request('GET', 'document?id=' + sId + '&description=true&ticket=' + authTicket, true, function (res) {
          var descr = document.querySelector('.description');
          descr.innerHTML = res;
          loader.style.display = 'none';
      }, function (err) {
          console.error('document err:', err);
          loader.style.display = 'none';
      });
    }, false);
      // }
  }
}
