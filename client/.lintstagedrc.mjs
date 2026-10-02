// Frontend lint-staged config. Tasks run from client/, so they use this folder's ESLint and Prettier.
export default {
  // Lint with autofix first, then format. --max-warnings=0 means a warning blocks the commit too.
  '*.{ts,tsx,js,mjs}': ['eslint --fix --max-warnings=0', 'prettier --write'],
  '*.{json,css,html,md,yml,yaml}': 'prettier --write',
}
