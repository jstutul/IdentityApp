import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Account } from '../../services/account';
import { map, take } from 'rxjs';
import { Shared } from '../../services/shared';

export const authorizationGuard: CanActivateFn = (route, state) => {

  const accountService = inject(Account);
  const sharedService = inject(Shared);
  const router = inject(Router);

  return accountService.user$.pipe(
    take(1),
    map(user => {

      if (user) {
        return true;
      }

      sharedService.showNofication(
        false,
        'Restricted Area',
        'Please login first.'
      );

      return router.createUrlTree(['/accounts/login'], {
        queryParams: {
          returnUrl: state.url
        }
      });

    })
  );
};