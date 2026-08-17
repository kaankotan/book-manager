export const TITLE_MAX_LENGTH = 1024

export const DESCRIPTION_MAX_LENGTH = 1024

export function textFieldError(label: string, value: string, maxLength: number): string | null {
  if (value.trim().length === 0) {
    return `${label} is required`
  }

  return value.trim().length > maxLength
    ? `${label} must be ${maxLength} characters or fewer`
    : null
}
