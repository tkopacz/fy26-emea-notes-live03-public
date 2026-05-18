import assert from 'node:assert/strict'
import test from 'node:test'

import { getNoteId, isUuidV4, resolveNotePath } from './notePath.js'

test('visiting the root path creates a new UUID v4 note URL', () => {
  const resolved = resolveNotePath('/')

  assert.equal(resolved.redirected, true)
  assert.equal(isUuidV4(getNoteId(resolved.path)), true)
})

test('refreshing an existing note URL keeps the current path', () => {
  let createUuidCalls = 0
  const existingPath = '/a3d6d31e-2210-4e67-a33e-7efef4b0b2eb'

  const resolved = resolveNotePath(existingPath, () => {
    createUuidCalls += 1
    return 'should-not-be-used'
  })

  assert.deepEqual(resolved, {
    path: existingPath,
    redirected: false,
  })
  assert.equal(createUuidCalls, 0)
})
