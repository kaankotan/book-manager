import { IconAlignLeft, IconPencil, IconSparkles } from '@tabler/icons-react'
import type { ComponentType } from 'react'

type IconComponent = ComponentType<{ size?: number | string; stroke?: number }>

export type ChangeTypeAppearance = {
  label: string
  color: string
  icon: IconComponent
}

const APPEARANCE: Record<string, ChangeTypeAppearance> = {
  Created: { label: 'Created', color: 'teal', icon: IconSparkles },
  TitleChanged: { label: 'Title changed', color: 'ink', icon: IconPencil },
  DescriptionChanged: { label: 'Description changed', color: 'orange', icon: IconAlignLeft },
}

function humanize(changeType: string): string {
  const spaced = changeType.replace(/([a-z0-9])([A-Z])/g, '$1 $2')

  return spaced.charAt(0).toUpperCase() + spaced.slice(1).toLowerCase()
}

export function changeTypeAppearance(changeType: string): ChangeTypeAppearance {
  return APPEARANCE[changeType] ?? { label: humanize(changeType), color: 'gray', icon: IconPencil }
}

export function changeTypeColor(changeType: string): string {
  return changeTypeAppearance(changeType).color
}
