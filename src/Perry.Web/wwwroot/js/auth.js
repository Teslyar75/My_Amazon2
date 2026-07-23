(function () {
  var toggle = document.querySelector("[data-password-toggle]");
  if (!toggle) return;

  var input = document.querySelector("[data-password-input]");
  var icon = toggle.querySelector("img");
  if (!input || !icon) return;

  var openSrc = toggle.getAttribute("data-icon-open");
  var closedSrc = toggle.getAttribute("data-icon-closed");

  toggle.addEventListener("click", function () {
    var show = input.type === "password";
    input.type = show ? "text" : "password";
    icon.src = show ? openSrc : closedSrc;
    icon.alt = show ? "Hide password" : "Show password";
  });
})();
