// Write a generated plan document, or check that it is current.
//
// A generated document is current when what the generator would write equals
// what is on disk, ignoring line endings and the dates in review-record rows.
// When only a date differs the file is left alone, so regenerating does not
// move the date of a review nobody repeated.
//
//   writeGenerated(path, content)            write when the content changed
//   writeGenerated(path, content, { check })  exit 1 when the content changed
const fs = require('fs');

const DATE_ROW = /^\| \d{4}-\d{2}-\d{2} \|/;
const normalise = (text) => text.replace(/\r\n/g, '\n').split('\n').map((l) => (DATE_ROW.test(l) ? l.replace(/^\| \d{4}-\d{2}-\d{2} \|/, '| DATE |') : l)).join('\n').trimEnd();

function writeGenerated(path, content, { check = process.argv.includes('--check') } = {}) {
  const before = fs.existsSync(path) ? fs.readFileSync(path, 'utf8') : null;
  const same = before !== null && normalise(before) === normalise(content);
  if (same) { if (check) console.log('current: ' + path); return false; }
  if (check) {
    const a = (before || '').replace(/\r\n/g, '\n').split('\n');
    const b = content.replace(/\r\n/g, '\n').split('\n');
    let i = 0;
    while (i < a.length && i < b.length && normalise(a[i]) === normalise(b[i])) i++;
    console.log('stale: ' + path + ' differs from its generator from line ' + (i + 1));
    process.exit(1);
  }
  fs.writeFileSync(path, content, 'utf8');
  return true;
}

module.exports = { writeGenerated, normalise };
