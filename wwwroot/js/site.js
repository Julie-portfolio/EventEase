// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//
// Simple reveal-on-scroll and image fade-in
(function(){
  'use strict';
  function reveal() {
    var els = document.querySelectorAll('.reveal-on-scroll');
    var images = document.querySelectorAll('.img-fade');
    var windowHeight = window.innerHeight;
    els.forEach(function(el){
      var rect = el.getBoundingClientRect();
      if(rect.top <= windowHeight - 100){
        el.classList.add('visible');
      }
    });
    images.forEach(function(img){
      var rect = img.getBoundingClientRect();
      if(rect.top <= windowHeight - 80){
        img.classList.add('visible');
      }
    });
  }

  document.addEventListener('DOMContentLoaded', function(){
    reveal();
    window.addEventListener('scroll', reveal);
  });
})();
