import {
  Anchor,
  Badge,
  Paper,
  Table,
  createTheme,
  rem,
  type MantineColorsTuple,
} from '@mantine/core'

const forest: MantineColorsTuple = [
  '#f2f7f2',
  '#e3ede4',
  '#c5d9c8',
  '#a3c3a8',
  '#82ad89',
  '#639872',
  '#45805a',
  '#3d7550',
  '#2f5f41',
  '#234b33',
]

const gold: MantineColorsTuple = [
  '#fdf8e6',
  '#f9efc9',
  '#f2dd94',
  '#eaca5c',
  '#e4ba31',
  '#dfae1a',
  '#d9a30c',
  '#b98a05',
  '#9c7400',
  '#7f5f00',
]

const gray: MantineColorsTuple = [
  '#f8f6f1',
  '#efece4',
  '#ded9cd',
  '#c8c2b3',
  '#aca597',
  '#8d8779',
  '#726c60',
  '#5a554b',
  '#413d36',
  '#2a2722',
]

const dark: MantineColorsTuple = [
  '#f6f3ec',
  '#e2ded4',
  '#c1bcb1',
  '#9c968a',
  '#6d6961',
  '#4c483f',
  '#332f29',
  '#22201b',
  '#191713',
  '#100f0c',
]

const bookFamily =
  "'Iowan Old Style', 'Palatino Linotype', Palatino, 'Book Antiqua', Georgia, 'Times New Roman', Times, serif"

export const theme = createTheme({
  primaryColor: 'forest',
  primaryShade: { light: 7, dark: 6 },
  colors: { forest, gold, gray, dark },
  autoContrast: true,
  defaultRadius: 'md',
  fontFamily: bookFamily,
  fontFamilyMonospace: "'Cascadia Mono', 'SF Mono', Menlo, Consolas, monospace",
  headings: {
    fontFamily: bookFamily,
    fontWeight: '700',
    sizes: {
      h1: { fontSize: rem(33), lineHeight: '1.2' },
      h2: { fontSize: rem(25), lineHeight: '1.28' },
      h3: { fontSize: rem(19), lineHeight: '1.35' },
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
