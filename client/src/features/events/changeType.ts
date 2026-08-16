const COLORS: Record<string, string> = {
  Created: 'green',
  TitleChanged: 'blue',
  DescriptionChanged: 'grape',
}

export function changeTypeColor(changeType: string): string {
  return COLORS[changeType] ?? 'gray'
}
