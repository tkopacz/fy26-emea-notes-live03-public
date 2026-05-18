const ROOT_PATH = '/'
const UUID_V4_PATTERN =
  /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

export function createNotePath(createUuid = () => crypto.randomUUID()) {
  return `/${createUuid()}`
}

export function resolveNotePath(
  pathname,
  createUuid = () => crypto.randomUUID(),
) {
  const normalizedPath = pathname?.startsWith(ROOT_PATH)
    ? pathname
    : ROOT_PATH

  if (normalizedPath === ROOT_PATH) {
    return {
      path: createNotePath(createUuid),
      redirected: true,
    }
  }

  return {
    path: normalizedPath,
    redirected: false,
  }
}

export function getNoteId(pathname) {
  return pathname.replace(/^\/+/, '')
}

export function isUuidV4(value) {
  return UUID_V4_PATTERN.test(value)
}
