module.exports = function (wallaby) {
  wallaby.defaults.files.instrument = true;
  wallaby.defaults.tests.instrument = true;
  return {
    files: [
      { pattern: 'src/**/*.ts', instrument: true },
      'src/**/*.html',
      'src/**/*.json',
      'test/**/*.json',
      'test/unit/**/__snapshots__/*.snap',
      'tsconfig.json',
      'test/jest-pretest.ts'
    ],

    tests: [
      'test/unit/**/*.spec.ts'
    ],

    compilers: {
      '**/*.ts': wallaby.compilers.typeScript({
        module: 'commonjs'
      })
    },

    env: {
      type: 'node',
      runner: 'node'
    },

    testFramework: 'jest'
  };
};
