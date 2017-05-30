'use strict';

var path = require('path');
var gulp = require('gulp');
var conf = require('./conf');
var userconf = require('./userconf');
var runSequence = require('run-sequence');
var inquirer = require('inquirer');
var _ = require('lodash');

var destinations = [];
var err = conf.errorHandler('Dev');

gulp.task('dev', function() {
  if (userconf.sites == undefined) {
    err('No sites defined in "gulp/userconf.js"');
    return;
  }
  var questions =
    [
      {
        type: 'checkbox',
        name: 'destinations',
        message: 'Select destinations:',
        choices: userconf.sites
      }
    ];
    setTimeout(function () {
      inquirer.prompt(questions).then(function (answers) {
        _.each(answers.destinations, function (a) {
          var site = _.find(userconf.sites, ['name', a]);
          destinations.push(site)
        });
      if (destinations.length == 0) {
        err('No destinations selected - exiting');
        return;
      }
      runSequence('dev-watch');
    });
    }, 0);
});

gulp.task('dev-watch', ['dev-all'], function(cb) {
    gulp.watch(path.join(conf.paths.connectors, 'bin/Debug/' + conf.patterns.dllPattern), ['dev-dll']);
    cb();
});

gulp.task('dev-dll', function() {
  _.forEach(destinations, function (d) {
    gulp.src(path.join(conf.paths.connectors, 'bin/Debug/' + conf.patterns.dllPattern))
    .pipe(gulp.dest(path.join(d.folder, '/bin/')));
  });
});

gulp.task('dev-all', ['dev-dll']);