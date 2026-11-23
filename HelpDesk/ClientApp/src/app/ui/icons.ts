import { Building2, Calendar, ChartColumn, CircleCheck, Hourglass, Inbox, LayoutDashboard, LogOut, Mail, Menu, Phone, Ticket, Unlink, User, Users, X, Check } from 'lucide';

type IconNode = [string, Record<string, string | number>][];

// Only the icons the application draws, so that the rest of the Lucide set is not bundled.
export const ICONS: Record<string, IconNode> = {
  'layout-dashboard': LayoutDashboard as IconNode,
  'ticket': Ticket as IconNode,
  'inbox': Inbox as IconNode,
  'hourglass': Hourglass as IconNode,
  'circle-check': CircleCheck as IconNode,
  'calendar': Calendar as IconNode,
  'users': Users as IconNode,
  'chart-column': ChartColumn as IconNode,
  'building-2': Building2 as IconNode,
  'user': User as IconNode,
  'menu': Menu as IconNode,
  'log-out': LogOut as IconNode,
  'x': X as IconNode,
  'check': Check as IconNode,
  'phone': Phone as IconNode,
  'mail': Mail as IconNode,
  'unlink': Unlink as IconNode
};
