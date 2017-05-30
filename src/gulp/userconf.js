/**
 *  This file contains user specific configuration settings
 */

/**
 *  These are the paths of destinations to copy deploy to
 *  Sites is an array of objects:
 *  { name: 'displayname', folder: 'c:/path/to/siteroot', checked: false }
 *  
 *  Parameters:
 *  'name': name shown in list (don't use fancy characters - haven't tested that ..so.. just don't)
 *  'folder': path to the site (no trailing slash and please use forward slashes - backslash needs to be escaped)
 *  'checked': (optional bool) selected by default
 */

/**
 *  IMPORTANT NOTE!
 *  If you change this file to fit your local setup, please make sure not to commit the changes.
 *  You can flag this file as being "unwatched" in your local git repository by using:
 *  
 *  git update-index --assume-unchanged Umbraco.Deploy.Contrib/gulp/userconf.js
 *  
 *  This will ensure any changes to this file will always be ignored when you commit.
 */
exports.sites = [
    { name: 'deploy1', folder: 'c:/temp/deploy1', checked: true },
    { name: 'deploy2', folder: 'c:/temp/deploy2', checked: true }
]