/**
 *  This file contains the variables used in other gulp files
 *  which defines tasks
 *  By design, we only put there very generic config values
 *  which are used in several places to keep good readability
 *  of the tasks
 */

var gutil = require('gulp-util');

/**
 *  The main paths of your project handle these with care
 */
exports.paths = {
  connectors: 'Umbraco.Deploy.Contrib.Connectors',
};
exports.patterns = {
  dllPattern: 'Umbraco.Deploy.Contrib*.{dll,pdb}'
}

/**
 *  Common implementation for an error handler of a Gulp plugin
 */
exports.errorHandler = function(title) {
  'use strict';
  return function(err) {
    gutil.log(gutil.colors.red('[' + title + ']'), err.toString());
    if (this !== undefined) {
      this.emit('end');
    }
  };
};