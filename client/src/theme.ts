import {
  Anchor,
  Badge,
  Paper,
  Table,
  createTheme,
  rem,
  type MantineColorsTuple,
} from '@mantine/core'

const ink: MantineColorsTuple = [
  '#f0f1fe',
  '#e1e2f2',
  '#bfc2e4',
  '#9b9fd6',
  '#7c82ca',
  '#6970c3',
  '#5f67c1',
  '#4f56aa',
  '#454c99',
  '#394186',
]

export const theme = createTheme({
  primaryColor: 'ink',
  primaryShade: { light: 6, dark: 5 },
  colors: { ink },
  defaultRadius: 'md',
  headings: {
    fontWeight: '700',
    sizes: {
      h1: { fontSize: rem(30), lineHeight: '1.25' },
      h2: { fontSize: rem(23), lineHeight: '1.3' },
      h3: { fontSize: rem(18), lineHeight: '1.35' },
    },
  },
  components: {
    Paper: Paper.extend({
      defaultProps: { bg: 'var(--mantine-color-default)' },
    }),
    Table: Table.extend({
      defaultProps: { verticalSpacing: 'sm', horizontalSpacing: 'md' },
    }),
    Badge: Badge.extend({
      defaultProps: { radius: 'sm', tt: 'none', fw: 600 },
    }),
    Anchor: Anchor.extend({
      defaultProps: { underline: 'never' },
    }),
  },
})
