import { useEffect, useState } from 'react'

const RELATIVE_FORMATTER = new Intl.RelativeTimeFormat(undefined, { numeric: 'auto' })

const CLOCK_FORMATTER = new Intl.DateTimeFormat(undefined, {
  hour: '2-digit',
  minute: '2-digit',
})

const UNITS: ReadonlyArray<[Intl.RelativeTimeFormatUnit, number]> = [
  ['second', 60],
  ['minute', 60],
  ['hour', 24],
  ['day', 7],
  ['week', 4.34524],
  ['month', 12],
  ['year', Number.POSITIVE_INFINITY],
]

const UNKNOWN_DATE_YEAR_THRESHOLD = 1000

function parse(value: string): Date | null {
  const date = new Date(value)

  return Number.isNaN(date.getTime()) ? null : date
}

export function formatRelativeTime(value: string, now: number): string {
  const date = parse(value)

  if (date === null) {
    return value
  }

  let delta = Math.min(0, (date.getTime() - now) / 1000)

  for (const [unit, limit] of UNITS) {
    if (Math.abs(delta) < limit) {
      return RELATIVE_FORMATTER.format(Math.round(delta), unit)
    }

    delta /= limit
  }

  return date.toLocaleDateString()
}

export function formatAbsoluteTime(value: string): string {
  return parse(value)?.toLocaleString() ?? value
}

export function formatClockTime(value: string): string {
  const date = parse(value)

  return date === null ? value : CLOCK_FORMATTER.format(date)
}

export function formatPublishedDate(value: string): string | null {
  const date = parse(value)

  if (date === null || date.getFullYear() < UNKNOWN_DATE_YEAR_THRESHOLD) {
    return null
  }

  return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

export function todayAsDateInputValue(): string {
  const now = new Date()
  const month = `${now.getMonth() + 1}`.padStart(2, '0')
  const day = `${now.getDate()}`.padStart(2, '0')

  return `${now.getFullYear()}-${month}-${day}`
}

export function useNow(intervalMs: number): number {
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    const timer = setInterval(() => setNow(Date.now()), intervalMs)

    return () => clearInterval(timer)
  }, [intervalMs])

  return now
}
