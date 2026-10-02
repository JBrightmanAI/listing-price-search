// Root lint-staged config: backend files. Frontend files are handled by client/.lintstagedrc.mjs
// (lint-staged uses the config closest to each staged file, and runs its tasks from that folder).

/** @param {string[]} files */
const quote = (files) => files.map((file) => `"${file}"`).join(' ')

export default {
  // Formats only the staged C# files with the repo's .NET style rules; fixes are re-staged automatically.
  '**/*.cs': (files) => `dotnet format ListingSearch.slnx --include ${quote(files)}`,
}
